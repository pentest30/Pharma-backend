# RÈGLES MÉTIERS DÉTAILLÉES
## Modules Inventaire, Approvisionnement et Préparation de Commandes

**Version:** 1.0
**Date:** 23 Novembre 2025
**Projet:** Pharma-Backend

---

## TABLE DES MATIÈRES

1. [Module Inventaire (Inventory)](#1-module-inventaire-inventory)
2. [Module Approvisionnement (Procurement)](#2-module-approvisionnement-procurement)
3. [Module Préparation de Commandes (PreparationOrder)](#3-module-préparation-de-commandes-preparationorder)

---

# 1. MODULE INVENTAIRE (INVENTORY)

## 1.1 Structure des Stocks (InventSum)

### 1.1.1 Dimensions Uniques du Stock

Chaque enregistrement de stock est identifié par une combinaison unique de dimensions :

| Dimension | Description | Fichier Source |
|-----------|-------------|----------------|
| `OrganizationId` | Isolation multi-tenant | `InventSum.cs:11` |
| `SiteId` | Localisation physique du site | `InventSum.cs:78` |
| `WarehouseId` | Entrepôt | `InventSum.cs:81` |
| `ProductId` | Référence produit | `InventSum.cs:12` |
| `VendorBatchNumber` | Numéro de lot fournisseur | `InventSum.cs:35` |
| `InternalBatchNumber` | Numéro de lot interne | `InventSum.cs:39` |
| `Color` | Variante couleur | `InventSum.cs:48` |
| `Size` | Variante taille | `InventSum.cs:50` |
| `IsPublic` | Visibilité (détermine le type de zone) | `InventSum.cs:73` |

### 1.1.2 Champs de Quantité

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         STRUCTURE DES QUANTITÉS                         │
├─────────────────────────────────────────────────────────────────────────┤
│  PhysicalOnhandQuantity     = Quantité physique totale disponible       │
│  PhysicalReservedQuantity   = Quantité réservée pour commandes          │
│  PhysicalDispenseQuantity   = Quantité quota non dispensable            │
│  PhysicalAvailableQuantity  = OnHand - Reserved (calculé)               │
└─────────────────────────────────────────────────────────────────────────┘
```

**Règle de calcul :**
```
PhysicalAvailableQuantity = PhysicalOnhandQuantity - PhysicalReservedQuantity
```

---

## 1.2 Règles d'Assignation des Zones

### 1.2.1 Zones de Stockage (IDs Codés en Dur)

| ID Zone | Nom Zone | Usage |
|---------|----------|-------|
| `7BD42E23-...-1AFE5CEACB16` | Zone Fournisseur | Réception marchandises |
| `7BD42E21-...-1AFE5CEACB16` | Zone Vendable | Stock principal de vente |
| `7BD42E22-...-1AFE5CEACB16` | Zone Non-Vendable | Endommagé/Retours |

### 1.2.2 États de Stock

| ID État | Nom | Type | Description |
|---------|-----|------|-------------|
| `7BD32E21-...` | **Libéré** | Vendable | Produits non-quota disponibles à la vente |
| `7BD52E22-...` | **Non libéré** | Vendable | Produits quota en file de dispensation |
| `7BD62E23-...` | **Abîmé** | Non-vendable | Marchandises endommagées |
| `7BD72E23-...` | **Périmé** | Non-vendable | Produits expirés |
| `7BD82E23-...` | **Sans vignette** | Non-vendable | Produits sans étiquette |
| `7BD92E23-...` | **Instance** | Non-vendable | Échantillons |
| `7BD13E23-...` | **RAL** | Non-vendable | Retours/Traitement spécial |

### 1.2.3 Règles d'Assignation Automatique

**À la réception de marchandises :**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    RÈGLE D'ASSIGNATION DE ZONE                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   SI Produit.EstQuota == false :                                        │
│      → Zone = "Zone vendable"                                           │
│      → État = "Libéré"                                                  │
│      → InventSum.IsPublic = true                                        │
│      → InventSum.PhysicalOnhandQuantity += Quantité                     │
│                                                                         │
│   SI Produit.EstQuota == true :                                         │
│      → Zone = "Zone vendable"                                           │
│      → État = "Non libéré"                                              │
│      → InventSum.IsPublic = true                                        │
│      → InventSum.PhysicalDispenseQuantity += Quantité                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `Consumers/InventoryConsumer.cs:187-356`

---

## 1.3 Gestion des Lots (Batches)

### 1.3.1 Structure d'un Lot

| Champ | Type | Description |
|-------|------|-------------|
| `VendorBatchNumber` | string(100) | Numéro lot fournisseur |
| `InternalBatchNumber` | string(100) | Numéro lot interne système |
| `ExpiryDate` | DateTime? | Date d'expiration |
| `ProductionDate` | DateTime? | Date de fabrication |
| `BestBeforeDate` | DateTime? | DLUO |

### 1.3.2 Règle d'Unicité des Lots

```
Un lot est unique par :
  OrganizationId + InternalBatchNumber + VendorBatchNumber + ProductId
```

**Fichier source :** `InventCommandsHandler.cs:32-36`

### 1.3.3 Génération du Numéro de Lot Interne

Lors de la création de facture fournisseur, le numéro de lot interne est généré selon le regroupement :

```
Regroupement par :
  - Type d'emballage (Packing)
  - Montant remise (Discount)
  - Prix d'achat unitaire
  - Prix de vente
  - Numéro lot fournisseur
  - ID Fournisseur
  - PpaHT
  - PFS
  - ID Organisation
  - Date d'expiration

Format : {VendorBatchNumber}_{compteur}
Exemple : LOT2024-001_1, LOT2024-001_2
```

---

## 1.4 Implémentation FEFO (First Expire First Out)

### 1.4.1 Règle de Tri par Date d'Expiration

**Pour les requêtes stock client B2B :**
```csharp
.OrderBy(x => x.ExpiryDate)  // Les plus proches de l'expiration en premier
```

**Fichier source :** `InventSumQueriesHandler.cs:234,261,414`

### 1.4.2 Validation de la Date d'Expiration

```
Le stock est visible UNIQUEMENT SI :
  (ExpiryDate == null) OU (ExpiryDate > DateTime.Now)
```

**Fichier source :** `InventSumQueriesHandler.cs:187,245,259,306`

### 1.4.3 FEFO pour Dispatch Quota (FIFO par expiration)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      DISPATCH FIFO QUOTA                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Récupérer tous les InventSum avec PhysicalDispenseQuantity > 0     │
│   2. Filtrer : ExpiryDate > DateTime.Now ET IsPublic = true             │
│   3. Trier par ExpiryDate ASCENDANT (plus ancien en premier)            │
│   4. Pour chaque InventSum (par ordre d'expiration) :                   │
│      SI InventSum.PhysicalDispenseQuantity >= QuantitéDemandée :        │
│         → PhysicalOnhandQuantity += QuantitéDemandée                    │
│         → PhysicalDispenseQuantity -= QuantitéDemandée                  │
│         → STOP                                                          │
│      SINON :                                                            │
│         → PhysicalOnhandQuantity += PhysicalDispenseQuantity            │
│         → Reste = QuantitéDemandée - PhysicalDispenseQuantity           │
│         → PhysicalDispenseQuantity = 0                                  │
│         → CONTINUER avec le lot suivant                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `InventQuotaCommandsHandler.cs:39-106`

### 1.4.4 LIFO pour Libération Quota

Pour retourner le stock quota de vendable vers non-dispensable :
```csharp
.OrderByDescending(x => x.ExpiryDate)  // Retourne les articles les plus récents d'abord
```

**Fichier source :** `InventQuotaCommandsHandler.cs:155,205-229`

---

## 1.5 Logique de Réservation

### 1.5.1 Mécanisme de Réservation

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        PROCESSUS DE RÉSERVATION                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Localiser InventSum par ID                                         │
│   2. Ajouter à PhysicalReservedQuantity                                 │
│   3. Valider : PhysicalAvailableQuantity >= 0                           │
│      SI NON : Exception("Quantité disponible ne peut pas être négative")│
│   4. Sauvegarder en base de données                                     │
│   5. Mettre à jour le cache Redis                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `InventSumCommandsHandler.cs:329-379`

### 1.5.2 Libération des Réservations

**Trois versions avec sophistication croissante :**

| Version | Clé de Recherche | Thread-Safe |
|---------|------------------|-------------|
| V1 | InventSum ID (dictionnaire) | Non |
| V2 | ProductId + InternalBatchNumber | Non |
| V3 | ProductId + InternalBatchNumber | Oui (SemaphoreSlim) |

**Règle de Clamp :**
```csharp
if (PhysicalReservedQuantity >= QuantitéÀLibérer)
    PhysicalReservedQuantity -= QuantitéÀLibérer;
else
    PhysicalReservedQuantity = 0;  // Clamp à 0
```

**Fichier source :** `ReleaseReservedQuantitiesCommandHandler.cs:33-119`

---

## 1.6 Synchronisation Cache (InventSum)

### 1.6.1 Stratégie de Cache Hybride

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      ARCHITECTURE CACHE                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   BASE DE DONNÉES (SQL Server)                                          │
│   └─ Source de vérité pour tous les champs                              │
│                                                                         │
│   CACHE REDIS                                                           │
│   └─ Clé : ProductId + OrganizationId                                   │
│   └─ Valeur : CachedInventSumCollection                                 │
│   └─ Maintient : PhysicalReservedQuantity pour gestion des commandes    │
│                                                                         │
│   CONTRAINTE CRITIQUE :                                                 │
│   "Ne JAMAIS écraser la quantité réservée lors du rechargement cache"   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.6.2 Processus de Synchronisation

```
1. Charger tous les InventSum depuis la base
2. Pour chaque combinaison produit + organisation :
   a. Vérifier si le cache existe ; créer sinon
   b. Trouver l'entrée correspondante par ProductId + InternalBatchNumber
   c. SI trouvée : PRÉSERVER la quantité réservée, mettre à jour les autres champs
   d. SI non trouvée : ajouter nouvelle entrée au cache
3. Nettoyer les entrées supprimées du cache
```

**Fichier source :** `InventSumCommandsHandler.cs:414-500`

### 1.6.3 Détection des Anomalies

```csharp
SI (cache.PhysicalReservedQuantity - db.PhysicalReservedQuantity != 0) :
    → Logger l'anomalie (ProductCode + InternalBatchNumber)
    → Stocker dans Redis : clé "reservation_anomaly"
```

**Fichier source :** `InventSumCommandsHandler.cs:620-640`

---

## 1.7 Types de Transactions et Effets sur le Stock

### 1.7.1 Énumération des Types de Transaction

| Code | Type | Description | Effet Stock |
|------|------|-------------|-------------|
| 10 | `SupplierReception` | Réception fournisseur | +Quantité |
| 20 | `SupplierInvoice` | Facture fournisseur | +Quantité |
| 30 | `CustomerReturn` | Retour client | +Quantité |
| 40 | `Readjustment` | Ajustement manuel | ±Quantité |
| 50 | `InterUnitTransfer` | Transfert inter-unités | ±Quantité |
| 60 | `DeliveryNote` | Bon de livraison | -Quantité |
| 70 | `CustomerInvoice` | Facture client | -Quantité |
| 80 | `Incineration` | Destruction | -Quantité |
| 90 | `Transfer` | Transfert manuel | ±Quantité |
| 100 | `ManualTransfer` | Transfert manuel enregistré | ±Quantité |

**Fichier source :** `Entities/InventItemTransaction.cs:39-52`

### 1.7.2 Champs de Transaction

```
InventId           : Emplacement physique du stock
Quantity           : Montant (positif ou négatif)
OriginQuantity     : Quantité avant transaction
NewQuantity        : Quantité après transaction
TransactionType    : Type (enum ci-dessus)
StockEntry         : true = entrée stock, false = sortie
RefDoc             : Document de référence (n° réception, n° commande, etc.)
OrderDate          : Date de la transaction d'origine
```

---

## 1.8 Effets du Statut BL (Bon de Livraison)

### 1.8.1 Matrice des Effets par Statut

| Statut | Code | Effet OnHand | Effet Reserved | Description |
|--------|------|--------------|----------------|-------------|
| **Valid** | 10 | -Quantité | -Quantité | Marchandises expédiées |
| **Deleted** | 20 | Aucun | -Quantité | Commande annulée, libération stock |
| **New** | 30 | -Quantité | Aucun | Nouvelle commande sans réservation préalable |
| **Updated** | 40 | -NouvelleQté | -AncienneQté | Modification de quantité |

**Fichier source :** `InventBlCommandsHandler.cs:67-99`

### 1.8.2 Logique Détaillée

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     STATUT 10 - LIGNE BL VALIDE                         │
├─────────────────────────────────────────────────────────────────────────┤
│   InventSum.PhysicalOnhandQuantity -= Quantité                          │
│   InventSum.PhysicalReservedQuantity -= Quantité                        │
│   Invent.PhysicalQuantity -= Quantité                                   │
│   Invent.PhysicalReservedQuantity -= Quantité                           │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                   STATUT 20 - LIGNE BL SUPPRIMÉE                        │
├─────────────────────────────────────────────────────────────────────────┤
│   InventSum.PhysicalReservedQuantity -= Quantité                        │
│   (Stock physique inchangé - libération de la réservation uniquement)   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                    STATUT 30 - NOUVELLE LIGNE BL                        │
├─────────────────────────────────────────────────────────────────────────┤
│   SI (PhysicalOnhandQuantity - Quantité < 0) :                          │
│       PhysicalOnhandQuantity = 0                                        │
│   SINON :                                                               │
│       PhysicalOnhandQuantity -= Quantité                                │
│   Invent.PhysicalQuantity -= Quantité                                   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                  STATUT 40 - LIGNE BL MISE À JOUR                       │
├─────────────────────────────────────────────────────────────────────────┤
│   InventSum.PhysicalOnhandQuantity -= NouvelleQuantité                  │
│   InventSum.PhysicalReservedQuantity -= AncienneQuantité                │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 1.9 Opérations de Transfert

### 1.9.1 États du Journal de Transfert

| Code | État | Description |
|------|------|-------------|
| 0 | `Created` | État initial |
| 1 | `Saved` | Modifications persistées |
| 2 | `Valid` | Exécuté (quantités mises à jour) |
| 3 | `Cancelled` | Annulé |
| 4 | `Removed` | Supprimé |

### 1.9.2 Validation de Transfert Zone à Zone

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   VALIDATION TRANSFERT INTER-ZONES                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Acquérir le verrou sur le produit (empêche modifications concur.)  │
│   2. Récupérer l'inventaire depuis zone source + état                   │
│   3. Vérifier le cache InventSum                                        │
│   4. Valider la disponibilité :                                         │
│      SI (zone = Vendable ET état = Libéré) :                            │
│         SI (Invent.PhysicalQuantity < Quantité) OU                      │
│            (InventSum.PhysicalAvailableQuantity < Quantité) :           │
│            → Erreur : "Quantité non disponible sur le stock..."         │
│                                                                         │
│   5. Exécution :                                                        │
│      a) Débiter source : Invent.PhysicalQuantity -= Quantité            │
│      b) Créer transaction négative                                      │
│      c) Créer/Incrémenter destination                                   │
│      d) Mettre à jour InventSum selon zone/état                         │
│      e) Mettre à jour le cache                                          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `ValidateTransferLogCommand.cs:74-126`

---

## 1.10 Règles de Validation

### 1.10.1 Contraintes de Quantité

| Règle | Condition | Message d'Erreur |
|-------|-----------|------------------|
| Non-négativité | OnHand < 0 ou Reserved < 0 | "Quantité ne peut pas être négative" |
| Disponibilité après réservation | AvailableQuantity < 0 | "Quantité disponible ne peut pas être négative" |
| Blocage cache | OnHand < 0 et Reserved cache > 0 | "Try later, the cached quantity has been temporarily reserved" |
| Suppression avec réservation | Reserved > 0 lors de la suppression | Impossible de supprimer |

### 1.10.2 Restrictions de Suppression InventSum

```
SI (cache.PhysicalReservedQuantity > 0) :
    → Erreur : impossible de supprimer

SI (db.PhysicalReservedQuantity == 0) :
    → Supprimer l'enregistrement
SINON :
    → db.PhysicalOnhandQuantity = db.PhysicalReservedQuantity
    → (Conserver l'enregistrement avec la quantité réservée)
```

**Fichier source :** `InventSumCommandsHandler.cs:262-277`

---

## 1.11 Concurrence et Thread Safety

### 1.11.1 Pattern Lock Provider

```csharp
await LockProvider<string>.ProvideLockObject(
    item.ProductId + orgId.Value.ToString()
).WaitAsync(cancellationToken);

try {
    // Opération critique
}
finally {
    LockProvider<string>.ProvideLockObject(...).Release();
}
```

**Utilisé pour :**
- Réservation/libération de stock
- Synchronisation du cache
- Validation de transfert
- Dispatch quota

### 1.11.2 Dictionnaire Concurrent

```csharp
private readonly ConcurrentDictionary<string, SemaphoreSlim> _concurrentDictionary;
```

**Fichier source :** `InventSumCommandsHandler.cs:34`

---

# 2. MODULE APPROVISIONNEMENT (PROCUREMENT)

## 2.1 Workflow des Commandes Fournisseurs

### 2.1.1 États des Commandes

| Code | État | Description |
|------|------|-------------|
| 10 | `Created` | État initial |
| 20 | `Saved` | Commande sauvegardée en base |
| 30 | `Accepted` | Acceptée par le fournisseur |
| 40 | `Processing` | En cours de traitement |
| 50 | `Shipping` | En cours d'expédition |
| 60 | `Completed` | Terminée |
| 70 | `Cancelled` | Annulée |
| 80 | `Rejected` | Rejetée |
| 90 | `Prepared` | Confirmée/Imprimée |
| 100 | `Consolidated` | Ordres de préparation consolidés |
| 110 | `InShippingArea` | En zone d'expédition |
| 120 | `Removed` | Supprimée (soft delete) |

### 2.1.2 Diagramme de Transition d'États

```
┌─────────┐     ┌───────┐     ┌──────────┐     ┌────────────┐     ┌──────────┐     ┌───────────┐
│ Created │────►│ Saved │────►│ Accepted │────►│ Processing │────►│ Shipping │────►│ Completed │
└─────────┘     └───────┘     └──────────┘     └────────────┘     └──────────┘     └───────────┘
                    │
                    │         ┌───────────┐
                    └────────►│ Cancelled │
                              └───────────┘

                              ┌──────────┐
                              │ Rejected │
                              └──────────┘
```

### 2.1.3 Format du Numéro de Commande

```
Format : "CF-" + AA + "/" + Séquence(10 chiffres)
Exemple : CF-24/0000000001

Où :
  - AA = Année sur 2 chiffres
  - Séquence = Numéro séquentiel paddé à 10 chiffres
```

**Fichier source :** `SupplierOrder.cs:64-65`

---

## 2.2 Règles de Création de Commande

### 2.2.1 Validation des Articles

| Champ | Règle | Message |
|-------|-------|---------|
| `ProductCode` | MaxLength(200), NotEmpty | Requis |
| `ProductId` | GUID non vide | Requis |
| `OrderId` | GUID non vide | Requis |
| `Quantity` | > 0 | Doit être supérieur à 0 |

**Fichier source :** `CreateSupplierOrderItem.cs:47-63`

### 2.2.2 Logique de Création en Cache

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CRÉATION COMMANDE EN CACHE                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Les commandes sont créées en cache Redis                           │
│      Clé : {OrgId}{UserId}{OrderId}                                     │
│      Lookup : {OrgId}{UserId}_supplier-order                            │
│                                                                         │
│   2. État en cache : Draft                                              │
│   3. État en base : Saved (après SaveSupplierOrderCommand)              │
│                                                                         │
│   4. Récupération données produit :                                     │
│      - Prix unitaire : fourni OU récupéré du master produit             │
│      - Taux TVA : récupéré du master produit                            │
│      - Numéros de lot : fournisseur et interne tracés                   │
│                                                                         │
│   5. Gestion des remises :                                              │
│      - Remise extra : convertie en % (/ 100)                            │
│      - Remise standard : stockée en double                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `CreateSupplierOrderItem.cs:89-226`

### 2.2.3 Sauvegarde en Base de Données

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    SAUVEGARDE COMMANDE                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Verrouillage Sémaphore (génération numéro séquence thread-safe)    │
│                                                                         │
│   2. Nouvelle commande :                                                │
│      → Créer entité SupplierOrder                                       │
│      → Générer numéro séquence                                          │
│                                                                         │
│   3. Commande existante :                                               │
│      → Effacer et remplacer les articles                                │
│                                                                         │
│   4. Pour chaque article :                                              │
│      → RemainingQuantity = Quantity (initialisation)                    │
│                                                                         │
│   5. État final : Saved (20)                                            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `SaveSupplierOrderCommand.cs:61-122`

---

## 2.3 Règles d'Autorisation

### 2.3.1 Annulation de Commande

```
RÈGLE : Seul le créateur peut annuler la commande

SI (_currentUser.UserId != order.CreatedByUserId) :
    → Exception : "Utilisateur non autorisé"
```

**Fichier source :** `CancelOrderCommand.cs:58-59`

### 2.3.2 Retour à l'État Sauvegardé

```
CONDITION : La commande ne doit pas avoir de factures liées

SI (commande.Factures.Count > 0) :
    → Erreur : "Commande non trouvée ou liée à une ou plusieurs factures"
```

**Fichier source :** `ReturnToSavedStatusCommand.cs:59`

---

## 2.4 Workflow des Factures Fournisseurs

### 2.4.1 États des Factures

| Code | État | Description |
|------|------|-------------|
| 0 | `Created` | Nouvelle facture |
| 1 | `Saved` | Sauvegardée en base |
| 2 | `InProgress` | En traitement (lots créés, stock mis à jour) |
| 3 | `Closed` | Tous les articles reçus |
| 4 | `Valid` | Réception validée |
| 5 | `Removed` | Supprimée |

### 2.4.2 Format du Numéro de Facture

```
Format : "FF-" + AA + "/" + Séquence(10 chiffres)
Exemple : FF-24/0000000001
```

### 2.4.3 Détection des Doublons

```
Vérification par : RefDocument + InvoiceDate.Date + SupplierId

SI doublon trouvé (ID facture différent) :
    → Erreur : "Une facture avec la même référence existe déjà"
```

**Fichier source :** `CreateInvoiceItemCommand.cs:175-184`

---

## 2.5 Validation de Facture et Intégration Stock

### 2.5.1 Processus de Validation

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    VALIDATION FACTURE FOURNISSEUR                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. SUIVI DES QUANTITÉS                                                │
│      Pour chaque article facture :                                      │
│      → Trouver article commande correspondant par ProductId             │
│      → InvoicedQuantity += Quantité article facture                     │
│      → RemainingQuantity -= Quantité (min 0)                            │
│      → WaitForDelivery = true                                           │
│                                                                         │
│   2. RÈGLE COMPLÉTION COMMANDE                                          │
│      SI somme(RemainingQuantity) == 0 :                                 │
│         → OrderStatus = Completed                                       │
│                                                                         │
│   3. TRANSITION ÉTAT FACTURE                                            │
│      Created/Saved → InProgress                                         │
│                                                                         │
│   4. CRÉATION DES LOTS                                                  │
│      Pour chaque article, créer ou récupérer lot :                      │
│      - InternalBatchNumber, VendorBatchNumber, ProductId                │
│      - ProductFullName, PurchaseDiscountRatio, SalesUnitPrice           │
│      - OrderId, OrganizationId, SupplierId, RefDoc                      │
│                                                                         │
│   5. INTÉGRATION STOCK                                                  │
│      → Zone : "Zone Chez le fournisseur"                                │
│      → État : "RAL"                                                     │
│      → Créer transaction SupplierReception                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `ValidateInvoiceCommand.cs:45-189`

---

## 2.6 Workflow des Bons de Réception

### 2.6.1 Format du Numéro de Réception

```
Format : "BR-" + AA + "/" + Séquence(10 chiffres)
Exemple : BR-24/0000000001
```

### 2.6.2 Validation des Articles de Réception

| Champ | Règle |
|-------|-------|
| `ProductCode` | MaxLength(200), NotEmpty |
| `InvoiceNumber` | MaxLength(200), NotEmpty |
| `ProductId` | GUID non vide |
| `InvoiceId` | GUID non vide |
| `InvoiceDate` | Non par défaut |
| `Quantity` | > 0 |
| `DocRef` | NotEmpty |

### 2.6.3 Processus de Validation Réception

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    VALIDATION BON DE RÉCEPTION                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. VERROUILLAGE                                                       │
│      Acquérir verrou : receipt.InvoiceNumber + orgId                    │
│      (Empêche validation concurrente)                                   │
│                                                                         │
│   2. VALIDATION FACTURE                                                 │
│      - La facture doit exister et ne pas être fermée                    │
│      - Erreur : "Invoice was not found or it has been closed"           │
│                                                                         │
│   3. RÉCONCILIATION DES QUANTITÉS                                       │
│      Pour chaque article facture correspondant :                        │
│      (Match par : ProductId + InternalBatchNumber + UnitPrice)          │
│      → ReceivedQuantity += Quantité réception                           │
│      → RemainingQuantity -= Quantité réception                          │
│                                                                         │
│   4. COMPLÉTION FACTURE                                                 │
│      SI somme(RemainingQuantity) == 0 :                                 │
│         → InvoiceStatus = Closed                                        │
│                                                                         │
│   5. ÉTAT RÉCEPTION                                                     │
│      Created/Saved → Valid                                              │
│                                                                         │
│   6. PUBLICATION ÉVÉNEMENT                                              │
│      → IDeliveryReceiptSubmittedEvent                                   │
│      → Consommé par module Inventory pour mise à jour stock             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `ValidateReceiptCommand.cs:47-116`

---

## 2.7 Calcul des Prix

### 2.7.1 Champs de Prix

| Champ | Description |
|-------|-------------|
| `PurchaseUnitPrice` | Coût unitaire (non remisé) |
| `PurchasePriceIncDiscount` | Coût unitaire après remise |
| `Discount` | Taux de remise achat (%) |
| `PpaHT` | Prix de revient HT |
| `PpaTTC` | Prix de revient TTC |
| `PFS` | Tarification spécifique (0, 1.5, 2.5) |
| `PpaPFS` | Prix de revient au niveau PFS |
| `SalePrice` | Prix de vente client |
| `WholesaleMargin` | Marge grossiste |
| `PharmacistMargin` | Marge pharmacien |

### 2.7.2 Formule PpaHT

```
PpaHT = PpaTTC / (1 + Taux_TVA%)
```

**Fichier source :** `CreateInvoiceItemCommand.cs:43`

---

## 2.8 Gestion des Produits Psychotropes

### 2.8.1 Traçage du Flag

```
Le flag Psychotropic est :
- Défini à la création de commande
- Stocké dans la commande en cache
- Persisté en base de données lors de la sauvegarde
- Propagé aux enregistrements stock/lot
- Utilisé pour le suivi réglementaire (pas de restriction de validation)
```

**Fichiers sources :**
- `SupplierOrder.cs:52`
- `CachedOrder.cs:33`
- `CreateSupplierOrderItem.cs:34,120`

---

## 2.9 Publication et Consommation d'Événements

### 2.9.1 Événement de Réception Validée

```
IDeliveryReceiptSubmittedEvent :
  - DeliveryReceiptId
  - CorrelationId (nouveau GUID)
  - ItemEvents (List<DeliveryItem>)
  - OrganizationId
  - UserId
  - RefDoc (numéro réception)
```

### 2.9.2 Consommateurs d'Événements

**DeliveryReceiptCreated :**
- Déclenché après validation réussie
- Notification SignalR : "Validation terminée avec succès"

**IdeliverReceiptCancelledEvent :**
- Déclenché en cas d'échec validation
- Rollback des quantités
- Notification SignalR : "Echec de la Validation"

**Fichier source :** `DeliveryReceiptConsumer.cs`

---

# 3. MODULE PRÉPARATION DE COMMANDES (PreparationOrder)

## 3.1 Génération des Ordres de Préparation

### 3.1.1 Génération du Numéro d'Identification

```
Format IdentifierNumber : "BL-" + AA + "/" + (padded)OrderIdentifier + "/" + ZoneGroupOrder
Format BarCode : AA + (padded)OrderIdentifier + (padded)ZoneGroupOrder

Exemples :
  IdentifierNumber : BL-24/000000002/01
  BarCode : 2400000000201
```

**Fichier source :** `PreparationOrderCommandHandler.cs:98-109`

### 3.1.2 Assignation des Groupes de Zones

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   ASSIGNATION GROUPE DE ZONES                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   SI article.ZoneGroupId est null/vide OU                               │
│      article.DefaultLocation est null/vide OU                           │
│      article.PickingZoneName est null/vide :                            │
│                                                                         │
│      1. Récupérer le produit depuis le module Catalogue                 │
│      2. Obtenir PickingZone et ZoneGroup du produit                     │
│                                                                         │
│      SI produit.PickingZone est null :                                  │
│         → Assigner Zone par défaut "X" (zone spéciale)                  │
│                                                                         │
│      3. Assigner :                                                      │
│         - ZoneGroupId, ZoneGroupName                                    │
│         - PickingZoneId, PickingZoneName                                │
│         - PickingZoneOrder                                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `PreparationOrderCommandHandler.cs:110-153`

### 3.1.3 Initialisation des Articles

```
article.Status = BlStatus.Valid (10)
article.OldQuantity = article.Quantity
article.PackingQuantity = article.Quantity / article.Packing (ou 0 si Packing = 0)
article.PreviousInternalBatchNumber = article.InternalBatchNumber
```

### 3.1.4 Calcul Total des Colis

```
TotalPackage = Somme(articles.PackingQuantity)
```

---

## 3.2 États des Ordres de Préparation

### 3.2.1 PreparationOrderStatus

| Code | État | Description |
|------|------|-------------|
| 10 | `Prepared` | État initial à la création de l'OP |
| 20 | `Controlled` | Tous les articles vérifiés |
| 30 | `Consolidated` | Toutes les zones consolidées |
| 40 | `Valid` | Réservé pour usage futur |
| 50 | `ReadyToBeShipped` | Après consolidation et réception expédition |
| 500 | `CancelledOrder` | Commande annulée |

### 3.2.2 BlStatus (Niveau Article)

| Code | État | Description |
|------|------|-------------|
| 10 | `Valid` | Article dans la commande, non modifié |
| 20 | `Deleted` | Article supprimé de l'OP, ne sera pas expédié |
| 30 | `New` | Article ajouté pendant la phase de contrôle |
| 40 | `Updated` | Lot ou quantité modifié pendant le contrôle |

### 3.2.3 Diagramme de Transition d'États

```
┌──────────┐     ┌────────────┐     ┌──────────────┐     ┌───────┐     ┌─────────────────┐
│ Prepared │────►│ Controlled │────►│ Consolidated │────►│ Valid │────►│ ReadyToBeShipped│
│   (10)   │     │    (20)    │     │     (30)     │     │ (40)  │     │      (50)       │
└──────────┘     └────────────┘     └──────────────┘     └───────┘     └─────────────────┘
     │
     │ (Annulation)
     ▼
┌────────────────┐
│ CancelledOrder │
│     (500)      │
└────────────────┘
```

---

## 3.3 Workflow de Contrôle/Vérification

### 3.3.1 Prérequis de Contrôle

```
RÈGLE : Transition vers CONTROLLED uniquement SI :
  - TOUS les PreparationOrderItems ont IsControlled = true
  - État actuel est Prepared (10) ou 0 (non initialisé)

SINON : Erreur "OP Non Controllé" - "Il existe des lignes non contrôlées !"
```

**Fichier source :** `ControlPreparationOrderCommand.cs:44-48`

### 3.3.2 Assignation des Agents aux Zones

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ASSIGNATION DES AGENTS                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   AddAgentsCommand assigne :                                            │
│   - ExecutedById/ExecutedByName : Personne ayant prélevé les articles   │
│   - VerifiedById/VerifiedByName : Personne ayant vérifié le prélèvement │
│   - PickingZoneId/PickingZoneName : Zone concernée                      │
│                                                                         │
│   Logique :                                                             │
│   SI exécuteur existe déjà pour la zone :                               │
│      → Mettre à jour ExecutedById, ExecutedByName, ExecutedTime         │
│   SINON :                                                               │
│      → Créer nouvel enregistrement PreparationOrderExecuter             │
│                                                                         │
│   SI vérificateur existe déjà pour la zone :                            │
│      → Mettre à jour VerifiedById, VerifiedByName, VerifiedTime         │
│   SINON :                                                               │
│      → Créer nouvel enregistrement PreparationOrderVerifier             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `PreparationOrderCommandHandler.cs:532-615`

### 3.3.3 Traitement des Modifications d'Articles

```
┌─────────────────────────────────────────────────────────────────────────┐
│                TRAITEMENT UpdatePreparationOrderCommand                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   VALIDATION :                                                          │
│   - PickingZoneId, VerifiedById, ExecutedById ne doivent pas être vides │
│   - État commande NE DOIT PAS être 70 (annulé) ou 200                   │
│   - Erreur : "l'agent de préparation est obligatoire"                   │
│                                                                         │
│   TRAITEMENT DES MODIFICATIONS :                                        │
│                                                                         │
│   a) NOUVEAUX ARTICLES (Status == BlStatus.New) :                       │
│      - Ajouter aux PreparationOrderItems                                │
│      - Empêcher doublons (même ProductId + InternalBatchNumber)         │
│                                                                         │
│   b) CHANGEMENTS DE LOT (InternalBatchNumber modifié) :                 │
│      - Marquer : IsControlled = true                                    │
│      - Mettre à jour : Quantity, Packing, Discount, ExpiryDate          │
│      - Préserver : PreviousInternalBatchNumber (lot commandé original)  │
│      - Définir Status = BlStatus.Updated (sauf si Deleted)              │
│                                                                         │
│   c) CHANGEMENTS DE QUANTITÉ :                                          │
│      - Marquer : IsControlled = true                                    │
│      - Mettre à jour : Quantity, Packing, Status                        │
│                                                                         │
│   ACTIONS POST-MISE À JOUR :                                            │
│   - Appeler ServiceAX2012Factory pour sauvegarder dans AX               │
│   - Libérer les quantités réservées pour les anciens lots               │
│   - Libérer les augmentations de quota si quantité réduite              │
│   - Déclencher AddAgentsCommand                                         │
│   - Déclencher ControlPreparationOrderCommand                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `UpdatePreparationOrderCommand.cs:91-207`

### 3.3.4 Libération des Quantités Réservées

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   LIBÉRATION QUANTITÉS RÉSERVÉES                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Pour chaque article avec OldQuantity > 0 :                         │
│      - Libérer l'ancienne quantité des réservations stock               │
│      - Utiliser PreviousInternalBatchNumber si disponible               │
│                                                                         │
│   2. Pour chaque produit (groupé par ProductId) :                       │
│      - Calculer : ancienne quantité totale - nouvelle quantité totale   │
│      - SI différence > 0 : Augmenter quota pour le client               │
│                                                                         │
│   3. Envoyer ReleaseReservedQuantityCommandV3 au module Inventory       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `UpdatePreparationOrderCommand.cs:241-288`

---

## 3.4 Génération des Bons de Livraison

### 3.4.1 Prérequis pour la Création

```
CONDITIONS :
1. ConsolidationOrder doit exister pour la commande
2. DeliveryOrder NE DOIT PAS déjà exister pour cet OrderId
   → Erreur : "Bl Already Generated"
3. Organization ID doit être valide

VERROUILLAGE : consolidationOrder.OrderIdentifier + orgId
```

**Fichier source :** `CreateDeleiveryOrderCommand.cs:76-99`

### 3.4.2 Format du Numéro de BL

```
Format : "BL-" + AA + "/" + Séquence(10 chiffres)
Exemple : BL-24/0000000001
```

### 3.4.3 Agrégation des Articles

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    AGRÉGATION ARTICLES BL                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Grouper tous les PreparationOrderItems par InternalBatchNumber     │
│   2. SOMMER les quantités par lot                                       │
│   3. EXCLURE les articles avec Status == BlStatus.Deleted               │
│                                                                         │
│   Pour chaque groupe :                                                  │
│   - Récupérer l'OrderItem original pour les données de prix             │
│   - Créer DeleiveryOrderItem avec :                                     │
│     * Produit : ProductId, ProductName, ProductCode                     │
│     * Prix : UnitPrice, PurchaseUnitPrice, Tax, Discount, ExtraDiscount │
│     * Lot : InternalBatchNumber, VendorBatchNumber                      │
│     * Quantité : Somme de tous les articles du lot                      │
│     * Infos emballage                                                   │
│     * Date d'expiration                                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `CreateDeleiveryOrderCommand.cs:121-143`

### 3.4.4 Publication d'Événement

```
IDeliveryOrderSubmittedEvent publié avec :
  - DeliveryOrderId
  - Détails DeliveryOrder
  - Commande originale
  - PreparationOrderItems de tous les OPs
  - OrganizationId
  - UserId
  - RefDoc (SequenceNumber)

Consommateur : Module Inventory pour la décrémentation de stock
```

**Fichier source :** `CreateDeleiveryOrderCommand.cs:147-158`

### 3.4.5 Création de Facture

```
Après création réussie du BL :
→ Envoyer CreateInvoiceCommand avec DeliveryOrderId
→ Déclenche la génération de facture dans le module Ventes
```

**Fichier source :** `CreateDeleiveryOrderCommand.cs:175`

---

## 3.5 Règles de Consolidation

### 3.5.1 Déclenchement de la Consolidation de Zone

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CONSOLIDATION DE ZONE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   ZoneConsolidationCommand :                                            │
│                                                                         │
│   1. Récupérer toutes les zones uniques dans l'ordre de préparation     │
│                                                                         │
│   2. Obtenir la liste des zones contrôlées :                            │
│      SI ByPassControlStep = true :                                      │
│         → Toutes les zones sont "consolidées"                           │
│      SINON :                                                            │
│         → Seules les zones avec vérificateurs sont "consolidées"        │
│                                                                         │
│   3. Vérifier si toutes les zones sont consolidées :                    │
│      Compter les zones avec vérificateurs/exécuteurs                    │
│      SI compteur == total zones uniques :                               │
│         a) Marquer PreparationOrderStatus = Consolidated                │
│         b) Vérifier si TOUS les OPs de la commande sont consolidés      │
│         c) SI oui : Créer ConsolidationOrder                            │
│         d) SI consolidation existe : Mettre à jour colis/responsables   │
│                                                                         │
│   4. Création ordre consolidation :                                     │
│      Sommer TotalPackage et TotalPackageThermolabile                    │
│      de tous les OPs de la commande                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `PreparationOrderCommandHandler.cs:741-848`

### 3.5.2 Création de l'Ordre de Consolidation

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CRÉATION ORDRE CONSOLIDATION                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. Mapper ConsolidationCommand vers entité ConsolidationOrder         │
│   2. Définir Consolidated = false initialement                          │
│   3. Définir ConsolidatedTime = DateTime.Now                            │
│   4. Persister en base de données                                       │
│                                                                         │
│   5. Déclencher PrintConsolidationOrderLabelCommand :                   │
│      - Générer étiquette de consolidation                               │
│      - Imprimer sur imprimante étiquettes                               │
│                                                                         │
│   6. Vérifier si tous les articles ont été supprimés :                  │
│      SI tous PreparationOrderItems.Status == BlStatus.Deleted :         │
│         → Mettre à jour OrderStatus = 70 (Annulé)                       │
│      SINON :                                                            │
│         → Mettre à jour OrderStatus = 100 (En Consolidation)            │
│                                                                         │
│   7. Envoyer facture à AX :                                             │
│      DOSI_SalesOrderServiceClient.invoiceAsync()                        │
│      Paramètres : CodeAx, TotalPackage, TotalPackageThermolabile        │
│      SI succès : OrderStatus = 140 (Facturé)                            │
│      SI échec : Retourner erreur avec message AX                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `ConsolidationCommandHandler.cs:61-90`

### 3.5.3 Validation des Colis

```
RÈGLE : SI TotalPackage + TotalPackageThermolabile == 0 :
    → Erreur : "NB Colis" - "toutes les lignes de la commande numéro {OrderId} ont été barrées"
    → Signifie que tous les articles de la commande ont été supprimés
```

**Fichier source :** `ConsolidationCommandHandler.cs:252-260`

### 3.5.4 Mise à Jour de la Consolidation

```
ConsolidationUpdateCommand :

1. Suivre si les colis ont changé :
   totalPackageChanged = (ancien != request.TotalPackage OU ancien != request.TotalPackageThermolabile)

2. Mettre à jour l'ordre de consolidation :
   - ConsolidatedByName/ConsolidatedById
   - ReceivedInShippingBy/ReceivedInShippingId
   - TotalPackage, TotalPackageThermolabile
   - Flag Consolidated
   - ReceptionExpeditionTime = DateTime.Now

3. Réimprimer étiquette SI :
   - NON request.Consolidated OU
   - ReceivedInShippingById est vide OU
   - Colis ont changé

4. Créer BL SI :
   - AXInterfacing = false
   - Pas de BL existant

5. Mettre à jour OrderStatus = 110 (En Expédition)

6. SI consolidé ET reçu en expédition :
   → OrderStatus = "ReadyToBeShipped" (via MakeOrderAsToBeShippedCommand)

7. SI colis ont changé :
   → Appeler UpdateTotalPackagesAX()
   → Erreur si mise à jour échoue
```

**Fichier source :** `ConsolidationCommandHandler.cs:142-197`

---

## 3.6 Génération et Impression des BL

### 3.6.1 Processus d'Impression

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    IMPRESSION BL (PrintBlCommand)                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1. CALCUL DES PAGES (algorithme complexe) :                           │
│      - Récupérer tous les PreparationOrders pour le même OrderId        │
│      - Grouper les articles par PickingZoneId                           │
│      - Calculer articles par zone                                       │
│      - Calculer sauts de page (A4 = 842 pixels hauteur)                 │
│        * En-tête : 50 + 160 pixels                                      │
│        * En-tête zone : 120 + 20 pixels                                 │
│        * Par article : 40 pixels                                        │
│        * Pied : 20 + 10 pixels                                          │
│      - Construire ZonesStringByBL                                       │
│                                                                         │
│   2. TRI DES ARTICLES :                                                 │
│      - Trier par DefaultLocation (null trié comme "ZZZ")                │
│      - Puis par ProductName                                             │
│                                                                         │
│   3. VALIDATION COMMANDE :                                              │
│      SI order.OrderStatus == 70 (annulé) :                              │
│         → Erreur : "Cette commande a été annulée par le service commercial" │
│                                                                         │
│   4. BYPASS ÉTAPE CONTRÔLE :                                            │
│      SI OpSettings.ByPassControlStep == true :                          │
│         → Définir PreparationOrderStatus = Controlled                   │
│                                                                         │
│   5. GÉNÉRATION PDF :                                                   │
│      - Créer PreparationOrderToPdfHelper                                │
│      - Générer bytes PDF                                                │
│                                                                         │
│   6. IMPRESSION :                                                       │
│      - Obtenir IP imprimante du groupe de zones                         │
│      - Appeler PrintHelper.PrintData(bytes, printerIP)                  │
│      - SI requête groupée : Ne pas mettre à jour BDD, juste imprimer    │
│      - SINON : Mettre à jour PrintedTime, PrintedBy, Printed=true,      │
│               PrintCount++                                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Fichier source :** `PreparationOrderCommandHandler.cs:274-448`

### 3.6.2 Contenu de l'Étiquette de Consolidation

```
L'étiquette de consolidation inclut :
- Nom du client
- Identifiant de commande
- Nombre total de colis
- Nombre total de colis thermolabiles
- Information secteur
- Zones impliquées dans la commande
- Code-barres (OrderIdentifier répété)
```

**Fichier source :** `ConsolidationCommandHandler.cs:268-277`

### 3.6.3 Suivi des Impressions

```
Chaque opération d'impression est tracée :
- Printed : flag booléen
- PrintedBy : UserId
- PrintedByName : Nom normalisé de l'utilisateur
- PrintedTime : DateTime
- PrintCount : Incrémenté à chaque impression
```

**Fichier source :** `PreparationOrderCommandHandler.cs:225-229`

---

## 3.7 Intégration avec les Autres Modules

### 3.7.1 Intégration Module Ventes (Orders)

```
Commandes envoyées au module Orders :
1. UpdateOrderStatusCommand avec status :
   - 70 : Annulé (si tous articles supprimés)
   - 100 : En consolidation
   - 110 : En expédition
   - 140 : Facturé

Requêtes au module Orders :
- GetOrderByIdQueryV2 : Vérifier état commande avant opérations
- GetSharedOrderById : Obtenir détails commande avec prix articles
- GetCustomerByOrganizationIdQuery : Obtenir info secteur client
```

### 3.7.2 Intégration Module Inventory

```
1. Pendant le contrôle (UpdatePreparationOrderCommand) :
   - ReleaseReservedQuantityCommandV3 envoyé
   - Contient : ProductId, InternalBatchNumber, Quantité à libérer
   - Utilisé quand : Lot changé ou quantité réduite

2. Gestion des quotas :
   - IncreaseQuotaCommandV2 envoyé si quantité réduite
   - Paramètres : CustomerId, ProductId, SalesPersonId, Quantity
   - Utilise cache Redis pour InventSumCreatedEvent

3. Décrémentation stock (via DeliveryOrder) :
   - IDeliveryOrderSubmittedEvent publié
   - Consommé par module Inventory
   - Déclenche DecrementStockCommand
   - Crée transaction stock
```

### 3.7.3 Intégration Dynamics AX

```
ServiceAX2012Factory :
- ServiceAX2012Factory.Create(PreparationOrderDtoV6, Order, Customer, pickingZoneId, pickingZoneName)
- Sauvegarde OP dans AX avec info statut
- Retourne CodeAx (référence AX)

Consolidation vers AX :
- DOSI_SalesOrderServiceClient.invoiceAsync(callContext, codeAx, totalPackage, thermolabile)
- callContext.Company = "HP"
- Credentials : _model.UserAx, _model.PasswordAx
- Gestion erreurs : Retourne messages erreur AX

Mise à jour colis vers AX :
- DOSI_SalesOrderServiceClient.updatePackageAsync()
- Met à jour compteurs colis quand consolidation change
```

---

## 3.8 Options de Configuration

```
OpSettings :
- ByPassControlStep : SI true, sauter la vérification manuelle (auto-marquer comme Controlled)
- RequireTDValidation : Usage futur (non implémenté actuellement)
```

**Fichier source :** `OpSettings.cs`

---

## 3.9 Validation du Code-Barres

```
Format validation code-barres :
- SI longueur > 13 : Tronquer à 13 caractères
- Extraire identifiant commande depuis position 2-11
- Extraire ordre zone depuis position 13
- Requêter par code-barres (13 premiers caractères uniquement)
```

**Fichier source :** `PreparationOrdersQueriesHandler.cs:91-107`

---

## RÉSUMÉ : FLUX PRINCIPAL DES OPÉRATIONS

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      FLUX COMPLET DE PRÉPARATION                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   VENTES                    PRÉPARATION                 INVENTAIRE      │
│   ───────                   ───────────                 ──────────      │
│                                                                         │
│   Commande Validée                                                      │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ CreatePreparationOrderCommand → OP créé (Prepared)              │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ PrintBlCommand → Impression BL pour préparateurs                │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ UpdatePreparationOrderCommand → Contrôle articles               │───┤
│   │ (Modification lots/quantités si nécessaire)                     │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                    │           │
│        │                                                    ▼           │
│        │                                    ┌───────────────────────┐   │
│        │                                    │ ReleaseReservedQty    │   │
│        │                                    │ IncreaseQuota         │   │
│        │                                    └───────────────────────┘   │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ ControlPreparationOrderCommand → Vérification complète (Controlled)│
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ ZoneConsolidationCommand → Toutes zones terminées (Consolidated)│   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ ConsolidationCommand → Création ConsolidationOrder              │   │
│   │ (Facture envoyée à AX, OrderStatus = 100/140)                   │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ ConsolidationUpdateCommand → Consolidé + Reçu en expédition     │   │
│   │ (OrderStatus = 110 → 50 ReadyToBeShipped)                       │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ CreateDeleiveryOrderCommand → Création BL                       │───┤
│   │ (Publie IDeliveryOrderSubmittedEvent)                           │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│        │                                                    │           │
│        │                                                    ▼           │
│        │                                    ┌───────────────────────┐   │
│        │                                    │ DecreaseStock         │   │
│        │                                    │ CreateTransaction     │   │
│        │                                    └───────────────────────┘   │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ CreateInvoiceCommand → Facture client générée                   │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

**Fin du Document des Règles Métiers Détaillées**

*Document généré automatiquement à partir de l'analyse approfondie du code source Pharma-Backend*
