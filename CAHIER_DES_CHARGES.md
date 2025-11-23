# CAHIER DES CHARGES FONCTIONNEL
## Système de Gestion Pharmaceutique - GHPCommerce

**Version:** 1.0
**Date:** 23 Novembre 2025
**Projet:** Pharma-Backend

---

## TABLE DES MATIERES

1. [Introduction](#1-introduction)
2. [Architecture Générale](#2-architecture-générale)
3. [Gestion des Utilisateurs et Permissions](#3-gestion-des-utilisateurs-et-permissions)
4. [Module Ventes (Sales)](#4-module-ventes-sales)
5. [Module Stock (Inventory)](#5-module-stock-inventory)
6. [Module Approvisionnement (Procurement)](#6-module-approvisionnement-procurement)
7. [Module Quotas](#7-module-quotas)
8. [Module Préparation de Commandes](#8-module-préparation-de-commandes)
9. [Module Ressources Humaines](#9-module-ressources-humaines)
10. [Module Actions Légales](#10-module-actions-légales)
11. [Intégrations Externes](#11-intégrations-externes)
12. [Exigences Non-Fonctionnelles](#12-exigences-non-fonctionnelles)

---

## 1. INTRODUCTION

### 1.1 Objectif du Document

Ce cahier des charges définit les spécifications fonctionnelles du système de gestion pharmaceutique GHPCommerce, une plateforme B2B de commerce de produits pharmaceutiques.

### 1.2 Périmètre du Système

Le système couvre l'ensemble de la chaîne logistique pharmaceutique :
- Gestion des commandes clients (pharmacies)
- Gestion des stocks et entrepôts
- Approvisionnement fournisseurs
- Gestion des quotas de produits réglementés
- Préparation et livraison des commandes
- Facturation et gestion financière

### 1.3 Parties Prenantes

| Rôle | Description |
|------|-------------|
| Grossiste-répartiteur | Opérateur principal du système |
| Pharmacies | Clients B2B passant des commandes |
| Fournisseurs | Laboratoires pharmaceutiques |
| Commerciaux | Force de vente terrain |
| Logisticiens | Préparateurs de commandes |

---

## 2. ARCHITECTURE GÉNÉRALE

### 2.1 Modules Fonctionnels

| Réf. | Module | Description |
|------|--------|-------------|
| MOD-01 | Sales | Gestion des ventes et commandes clients |
| MOD-02 | Inventory | Gestion des stocks et entrepôts |
| MOD-03 | Procurement | Approvisionnement fournisseurs |
| MOD-04 | Quota | Gestion des quotas produits |
| MOD-05 | PreparationOrder | Préparation et consolidation |
| MOD-06 | HumanResource | Gestion du personnel |
| MOD-07 | LegalActions | Actions légales et conformité |

### 2.2 Stack Technologique

| Composant | Technologie |
|-----------|-------------|
| Backend | .NET Core / ASP.NET Core |
| Base de données | SQL Server |
| ORM | Entity Framework Core |
| Message Broker | RabbitMQ + MassTransit |
| Cache | Redis |
| Authentification | IdentityServer4 (OAuth2/OIDC) |
| Temps réel | SignalR |
| Documentation API | Swagger/OpenAPI |

---

## 3. GESTION DES UTILISATEURS ET PERMISSIONS

### 3.1 Rôles Système

| Réf. | Rôle | Description |
|------|------|-------------|
| ROL-01 | SuperAdmin | Accès complet au système |
| ROL-02 | Admin | Administration générale |
| ROL-03 | SalesManager | Responsable commercial |
| ROL-04 | SalesPerson | Commercial terrain |
| ROL-05 | OnlineCustomer | Client en ligne (pharmacie) |
| ROL-06 | Supervisor | Superviseur des opérations |
| ROL-07 | ClaimManager | Gestionnaire des réclamations |
| ROL-08 | Buyer | Acheteur |
| ROL-09 | BuyerGroup | Groupe acheteurs |
| ROL-10 | PrintingAgent | Agent d'impression |
| ROL-11 | InventoryManager | Gestionnaire de stock |
| ROL-12 | Controller | Contrôleur qualité |
| ROL-13 | Consolidator | Consolidateur de commandes |
| ROL-14 | Executer | Exécutant (préparateur) |

### 3.2 Domaines de Permissions

| Réf. | Domaine | Modules Concernés |
|------|---------|-------------------|
| PERM-01 | Sales | Ventes, Commandes, Factures |
| PERM-02 | Catalog | Catalogue produits |
| PERM-03 | Inventory | Gestion des stocks |
| PERM-04 | Procurement | Approvisionnement |
| PERM-05 | Quota | Gestion des quotas |
| PERM-06 | Membership | Utilisateurs et rôles |
| PERM-07 | Hr | Ressources humaines |
| PERM-08 | Tiers | Clients et fournisseurs |

### 3.3 Matrice des Accès

| Fonctionnalité | SuperAdmin | Admin | SalesManager | SalesPerson | Supervisor | OnlineCustomer |
|----------------|------------|-------|--------------|-------------|------------|----------------|
| Créer commande | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Valider commande | ✓ | ✓ | ✓ | ✗ | ✓ | ✗ |
| Gérer factures | ✓ | ✓ | ✗ | ✓ | ✓ | ✗ |
| Gérer stocks | ✓ | ✓ | ✗ | ✗ | ✗ | ✗ |
| Créer avoirs | ✓ | ✓ | ✗ | ✗ | ✗ | ✗ |
| Gérer quotas | ✓ | ✓ | ✓ | ✗ | ✓ | ✗ |

---

## 4. MODULE VENTES (SALES)

### 4.1 Gestion des Commandes

#### 4.1.1 Création de Commande

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-001 | Le système DOIT permettre la création d'une commande brouillon par un pharmacien | Haute |
| SAL-002 | Le système DOIT associer une commande à un client (pharmacie) et un fournisseur | Haute |
| SAL-003 | Le système DOIT stocker la commande en cache Redis pendant l'édition (clé: SupplierId + UserId) | Haute |
| SAL-004 | Le système DOIT exiger un document de référence (RefDocument) pour les commandes de produits psychotropes | Haute |
| SAL-005 | Le système DOIT calculer automatiquement le total commande (somme des lignes TTC) | Haute |
| SAL-006 | Le système DOIT permettre l'ajout de plusieurs lignes de commande | Haute |
| SAL-007 | Le système DOIT afficher les commandes en cours pour un commercial donné | Moyenne |

#### 4.1.2 Lignes de Commande

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-008 | Chaque ligne DOIT contenir: ProductId, Quantité, Prix Unitaire, Remise | Haute |
| SAL-009 | Le système DOIT valider que la quantité est supérieure à 0 | Haute |
| SAL-010 | Le système DOIT appliquer les remises configurées (base + extra) | Haute |
| SAL-011 | Le système DOIT calculer le montant HT et TTC par ligne | Haute |
| SAL-012 | Le système DOIT permettre la modification des lignes avant envoi | Haute |
| SAL-013 | Le système DOIT permettre la suppression de lignes individuelles | Haute |

#### 4.1.3 Workflow des Commandes

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-014 | Le système DOIT implémenter le workflow suivant: Pending → Sent → Accepted → Processing → Shipping → Invoiced → Completed | Haute |
| SAL-015 | Le système DOIT permettre l'annulation d'une commande (état: Canceled) | Haute |
| SAL-016 | Le système DOIT permettre le rejet d'une commande (état: Rejected) | Haute |
| SAL-017 | Le système DOIT tracer les raisons d'annulation: ExceededDate, UnableFishingOrder, TestOrder | Moyenne |
| SAL-018 | Le système DOIT synchroniser les commandes avec Dynamics AX (états: CreatedOnAx, AxError) | Haute |
| SAL-019 | Le système DOIT gérer les créations partielles sur AX (état: PartiallyCreatedOnAX) | Moyenne |

**Diagramme d'états des commandes:**

```
┌─────────┐    ┌──────┐    ┌──────────┐    ┌────────────┐    ┌──────────┐
│ Pending │───►│ Sent │───►│ Accepted │───►│ Processing │───►│ Shipping │
└─────────┘    └──────┘    └──────────┘    └────────────┘    └──────────┘
     │              │                                              │
     │              ▼                                              ▼
     │         ┌──────────┐                                 ┌──────────┐
     │         │ Rejected │                                 │ Invoiced │
     │         └──────────┘                                 └──────────┘
     │                                                            │
     ▼                                                            ▼
┌──────────┐                                               ┌───────────┐
│ Canceled │                                               │ Completed │
└──────────┘                                               └───────────┘
```

#### 4.1.4 États de Paiement

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-020 | Le système DOIT gérer les états de paiement: Pending → Processing → Complete | Haute |
| SAL-021 | Le système DOIT permettre le rejet de paiement (état: Rejected) | Haute |
| SAL-022 | Le système DOIT marquer une commande comme payée | Haute |

### 4.2 Gestion des Remises

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-023 | Le système DOIT permettre de définir des remises par produit | Haute |
| SAL-024 | Le système DOIT appliquer les remises selon un seuil de quantité | Haute |
| SAL-025 | Le système DOIT gérer une période de validité (date début/fin) pour les remises | Haute |
| SAL-026 | Le système DOIT permettre les remises multiples sur une même commande | Moyenne |
| SAL-027 | Le système DOIT distinguer remise de base et remise extra | Moyenne |
| SAL-028 | Le système DOIT calculer: Remise globale = Somme(remises lignes) + Promotions + Packs | Haute |

### 4.3 Facturation

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-029 | Le système DOIT générer une facture depuis un bon de livraison validé | Haute |
| SAL-030 | Le système DOIT permettre l'impression de factures en PDF | Haute |
| SAL-031 | Le système DOIT permettre l'impression groupée de factures | Moyenne |
| SAL-032 | Le système DOIT exporter les factures au format Excel par client | Moyenne |
| SAL-033 | Le système DOIT suivre la dette client par facture | Haute |
| SAL-034 | Le système DOIT calculer le chiffre d'affaires par client | Haute |

**Contenu du PDF facture:**
- En-tête société (MED IJK SPA: NIS, AI, RC, NIF)
- Informations fournisseur (adresse, téléphone)
- Informations client
- Tableau des articles: N°, REF, DESIGNATION, LOT, DDP, QTE, PU HT, PT HT, MRG, PPA HT, PPA TTC, SHP, TVA

### 4.4 Avoirs (Credit Notes)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-035 | Le système DOIT permettre la création d'avoirs depuis une facture existante | Haute |
| SAL-036 | Le système DOIT exiger au moins un InvoiceId pour créer un avoir | Haute |
| SAL-037 | Le système DOIT tracer la raison de réclamation: Damaged (Endommagé), NotOrdered (Non commandé), Expired (Périmé) | Haute |
| SAL-038 | Le système DOIT enregistrer: numéro réclamation, note, date réclamation | Moyenne |
| SAL-039 | Le système DOIT gérer le flag retour produit (défaut: true) | Moyenne |
| SAL-040 | Le système DOIT implémenter le workflow: Draft → Validated → Canceled | Haute |
| SAL-041 | Le système DOIT mettre à jour le stock lors de la validation d'un avoir avec retour | Haute |

### 4.5 Transactions Financières

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-042 | Le système DOIT enregistrer les transactions de type: SalesInvoice, PurchaseInvoice, Refund | Haute |
| SAL-043 | Le système DOIT permettre le suivi de l'encours client | Haute |
| SAL-044 | Le système DOIT calculer le solde client en temps réel | Haute |

### 4.6 Panier d'Achat (E-commerce)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-045 | Le système DOIT permettre aux clients en ligne de gérer un panier | Moyenne |
| SAL-046 | Le système DOIT convertir le panier en commande à la validation | Moyenne |
| SAL-047 | Le système DOIT gérer les événements: GuestPickupCreatedEvent, GuestShipCreatedEvent | Basse |

### 4.7 Recherche et Filtrage

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-048 | Le système DOIT permettre la recherche de commandes avec filtres: date, client, statut | Haute |
| SAL-049 | Le système DOIT permettre le filtrage par flag psychotrope | Haute |
| SAL-050 | Le système DOIT paginer les résultats (Skip, Take) | Haute |
| SAL-051 | Le système DOIT permettre le tri multi-colonnes | Moyenne |
| SAL-052 | Le système DOIT fournir l'historique des commandes par code produit | Moyenne |

### 4.8 Impressions et Rapports

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| SAL-053 | Le système DOIT générer des PDF pour: Commandes, Factures, Avoirs | Haute |
| SAL-054 | Le système DOIT permettre l'impression groupée | Moyenne |
| SAL-055 | Le système DOIT fournir un rapport de CA par client | Moyenne |
| SAL-056 | Le système DOIT fournir un rapport de dette par client | Moyenne |

---

## 5. MODULE STOCK (INVENTORY)

### 5.1 Gestion des Stocks

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-001 | Le système DOIT permettre le suivi des quantités physiques par produit | Haute |
| INV-002 | Le système DOIT distinguer quantité physique et quantité réservée | Haute |
| INV-003 | Le système DOIT maintenir une vue agrégée des stocks (InventSum) | Haute |
| INV-004 | Le système DOIT synchroniser le cache Redis avec les stocks | Haute |
| INV-005 | Le système DOIT notifier en temps réel les changements de stock via SignalR | Haute |

### 5.2 Gestion des Lots (Batches)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-006 | Le système DOIT associer chaque stock à un numéro de lot | Haute |
| INV-007 | Le système DOIT enregistrer la date d'expiration par lot | Haute |
| INV-008 | Le système DOIT valider que la date d'expiration est supérieure à la date courante | Haute |
| INV-009 | Le système DOIT implémenter la règle FEFO (First Expire First Out) | Haute |
| INV-010 | Le système DOIT alerter sur les produits proches de péremption | Moyenne |

### 5.3 Zones de Stockage

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-011 | Le système DOIT permettre la définition de zones de stockage multiples | Haute |
| INV-012 | Le système DOIT classifier les zones par type (ZoneType) | Haute |
| INV-013 | Le système DOIT assigner automatiquement les produits quota à la zone "Non-libéré" | Haute |
| INV-014 | Le système DOIT assigner automatiquement les produits non-quota à la zone "Libéré" | Haute |
| INV-015 | Le système DOIT permettre la gestion des zones de picking | Moyenne |

### 5.4 États de Stock

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-016 | Le système DOIT gérer les états: Disponible, Réservé, Endommagé, Bloqué | Haute |
| INV-017 | Le système DOIT permettre le changement d'état de stock | Haute |
| INV-018 | Le système DOIT bloquer les mouvements sur stock bloqué | Haute |
| INV-019 | Le système DOIT tracer l'historique des changements d'état | Moyenne |

### 5.5 Mouvements de Stock

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-020 | Le système DOIT enregistrer les types de transaction: Transfer, SupplierReception, DeliveryNote, CustomerReturn | Haute |
| INV-021 | Le système DOIT créer une transaction pour chaque mouvement de stock | Haute |
| INV-022 | Le système DOIT tracer: date, quantité, utilisateur, motif | Haute |
| INV-023 | Le système DOIT permettre la consultation de l'historique des mouvements | Haute |

### 5.6 Transferts Inter-zones

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-024 | Le système DOIT permettre le transfert de stock entre zones | Haute |
| INV-025 | Le système DOIT créer un TransferLog pour chaque transfert | Haute |
| INV-026 | Le système DOIT valider la disponibilité avant transfert | Haute |
| INV-027 | Le système DOIT mettre à jour les deux zones de manière atomique | Haute |

### 5.7 Réceptions (Arrivals)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-028 | Le système DOIT enregistrer les réceptions de marchandises | Haute |
| INV-029 | Le système DOIT mettre à jour les stocks à la réception | Haute |
| INV-030 | Le système DOIT permettre la saisie des informations lot à la réception | Haute |

### 5.8 Réservations

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-031 | Le système DOIT réserver le stock lors de la validation de commande | Haute |
| INV-032 | Le système DOIT libérer les réservations lors de l'annulation | Haute |
| INV-033 | Le système DOIT vérifier: PhysicalQuantity >= RequestedQuantity avant réservation | Haute |
| INV-034 | Le système DOIT maintenir: PhysicalReservedQuantity séparé de PhysicalOnhandQuantity | Haute |

### 5.9 Règles Métier Stock

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INV-035 | Produits Quota: suivre PhysicalOnhandQuantity, PhysicalDispenseQuantity, PhysicalReservedQuantity | Haute |
| INV-036 | Produits Non-Quota: suivre PhysicalOnhandQuantity, PhysicalReservedQuantity | Haute |
| INV-037 | Le système DOIT rejeter les réservations si stock insuffisant | Haute |

---

## 6. MODULE APPROVISIONNEMENT (PROCUREMENT)

### 6.1 Commandes Fournisseurs

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRO-001 | Le système DOIT permettre la création de commandes fournisseurs | Haute |
| PRO-002 | Le système DOIT valider: Quantité > 0 pour chaque ligne | Haute |
| PRO-003 | Le système DOIT valider: Code produit max 200 caractères | Moyenne |
| PRO-004 | Le système DOIT gérer le document de référence pour produits psychotropes | Haute |
| PRO-005 | Le système DOIT appliquer les remises fournisseur (base + extra) | Moyenne |
| PRO-006 | Le système DOIT définir un prix unitaire par défaut si non spécifié | Moyenne |

### 6.2 Workflow Commandes Fournisseurs

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRO-007 | Le système DOIT implémenter: Created → Saved → Accepted → Processing → Shipping → Completed | Haute |
| PRO-008 | Le système DOIT permettre l'annulation (état: Cancelled) | Haute |
| PRO-009 | Le système DOIT permettre le rejet (état: Rejected) | Haute |
| PRO-010 | Le système DOIT gérer l'état Removed pour suppression logique | Moyenne |
| PRO-011 | Le système DOIT permettre la validation multi-étapes | Haute |

```
┌─────────┐    ┌───────┐    ┌──────────┐    ┌────────────┐    ┌──────────┐    ┌───────────┐
│ Created │───►│ Saved │───►│ Accepted │───►│ Processing │───►│ Shipping │───►│ Completed │
└─────────┘    └───────┘    └──────────┘    └────────────┘    └──────────┘    └───────────┘
                   │
                   ▼
              ┌───────────┐
              │ Cancelled │
              └───────────┘
```

### 6.3 Bons de Réception (Delivery Receipt)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRO-012 | Le système DOIT créer un bon de réception à la livraison fournisseur | Haute |
| PRO-013 | Le système DOIT valider chaque ligne avec un receipt ID valide | Haute |
| PRO-014 | Le système DOIT permettre la validation partielle | Moyenne |
| PRO-015 | Le système DOIT mettre à jour les stocks à la validation | Haute |
| PRO-016 | Le système DOIT publier IDeliveryReceiptCompletedEvent | Haute |
| PRO-017 | Le système DOIT permettre l'annulation (IdeliverReceiptCancelledEvent) | Moyenne |

### 6.4 Factures Fournisseurs

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRO-018 | Le système DOIT permettre la saisie des factures fournisseurs | Haute |
| PRO-019 | Le système DOIT implémenter: Created → Saved → InProgress → Closed → Valid | Haute |
| PRO-020 | Le système DOIT permettre le rapprochement avec les bons de réception | Haute |
| PRO-021 | Le système DOIT gérer l'état Removed pour suppression logique | Moyenne |
| PRO-022 | Le système DOIT calculer les écarts facture/réception | Moyenne |

---

## 7. MODULE QUOTAS

### 7.1 Gestion des Quotas

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| QUO-001 | Le système DOIT permettre l'allocation de quotas par client | Haute |
| QUO-002 | Le système DOIT permettre l'allocation de quotas par commercial | Haute |
| QUO-003 | Le système DOIT définir un état initial de quota (QuotaInitState) | Haute |
| QUO-004 | Le système DOIT suivre la consommation des quotas | Haute |
| QUO-005 | Le système DOIT bloquer les commandes si quota dépassé | Haute |

### 7.2 Demandes de Quota

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| QUO-006 | Le système DOIT permettre la création de demandes de quota | Haute |
| QUO-007 | Le système DOIT implémenter un workflow de validation (V1, V2, V3) | Haute |
| QUO-008 | Le système DOIT permettre le rejet de demandes | Haute |
| QUO-009 | Le système DOIT notifier en temps réel via QuotaNotification hub | Haute |
| QUO-010 | Le système DOIT tracer les transactions de quota | Haute |

### 7.3 Libération de Quotas

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| QUO-011 | Le système DOIT permettre la libération de quota (ReleaseQuotaCommand) | Haute |
| QUO-012 | Le système DOIT permettre la libération par client (ReleaseQuotaByCustomerCommand) | Moyenne |
| QUO-013 | Le système DOIT générer des PDF de quota client (PrintCustomerQuotaCommand) | Moyenne |

---

## 8. MODULE PRÉPARATION DE COMMANDES

### 8.1 Ordres de Préparation

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRE-001 | Le système DOIT générer des ordres de préparation depuis les commandes validées | Haute |
| PRE-002 | Le système DOIT assigner un préparateur (PreparationOrderExecuter) | Haute |
| PRE-003 | Le système DOIT assigner un vérificateur (PreparationOrderVerifier) | Haute |
| PRE-004 | Le système DOIT suivre l'état: Prepared → Controlled → Consolidated → Valid → ReadyToBeShipped | Haute |
| PRE-005 | Le système DOIT permettre l'annulation (état: CancelledOrder) | Haute |

```
┌──────────┐    ┌────────────┐    ┌──────────────┐    ┌───────┐    ┌─────────────────┐
│ Prepared │───►│ Controlled │───►│ Consolidated │───►│ Valid │───►│ ReadyToBeShipped│
└──────────┘    └────────────┘    └──────────────┘    └───────┘    └─────────────────┘
     │
     ▼
┌────────────────┐
│ CancelledOrder │
└────────────────┘
```

### 8.2 Contrôle Qualité

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRE-006 | Le système DOIT permettre le contrôle des ordres de préparation | Haute |
| PRE-007 | Le système DOIT valider chaque ligne préparée vs commande | Haute |
| PRE-008 | Le système DOIT signaler les écarts de quantité | Haute |
| PRE-009 | Le système DOIT permettre la validation ou le rejet | Haute |

### 8.3 Bons de Livraison (Delivery Order)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRE-010 | Le système DOIT générer un BL depuis un ordre de préparation validé | Haute |
| PRE-011 | Le système DOIT publier IDeliveryOrderSubmittedEvent pour réserver le stock | Haute |
| PRE-012 | Le système DOIT publier IDeliveryOrderCompletedEvent à la finalisation | Haute |
| PRE-013 | Le système DOIT permettre l'annulation (IDeliveryOrderCancelledEvent) | Haute |
| PRE-014 | Le système DOIT notifier via PreparationOrderHub: "Creation BL terminée avec succès" | Haute |

**Machine à états (DeliveryOrderStateMachine):**
```
DeliveryOrderSubmitted
  → Envoie DecreaseStockCommand (invent-queue)
  → État: Submitted

Pendant Submitted:
  → DeliveryOrderCompleted
    → Envoie DeliveryOrderCompletedCommand (preparation-order-queue)
    → État: Completed (final)

À tout moment:
  → DeliveryOrderCancelled
    → État: Rejected (final)
```

### 8.4 Consolidation

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRE-015 | Le système DOIT permettre la consolidation de plusieurs BL | Haute |
| PRE-016 | Le système DOIT générer des étiquettes de consolidation | Moyenne |
| PRE-017 | Le système DOIT suivre l'état BlStatus | Haute |
| PRE-018 | Le système DOIT permettre l'impression groupée des BL | Moyenne |

### 8.5 Impressions

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| PRE-019 | Le système DOIT générer les BL en PDF (PrintBlCommand) | Haute |
| PRE-020 | Le système DOIT permettre l'impression groupée (PrintBulkBlCommand) | Moyenne |
| PRE-021 | Le système DOIT imprimer les commandes en attente (PrintBulkPendingCommand) | Moyenne |
| PRE-022 | Le système DOIT générer les étiquettes de consolidation | Moyenne |
| PRE-023 | Le système DOIT générer les PDF d'ordres de préparation (GenerateOpPDFCommand) | Moyenne |

---

## 9. MODULE RESSOURCES HUMAINES

### 9.1 Gestion des Employés

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| HR-001 | Le système DOIT permettre la création d'employés | Haute |
| HR-002 | Le système DOIT permettre la modification des données employé | Haute |
| HR-003 | Le système DOIT permettre la suppression d'employés | Haute |
| HR-004 | Le système DOIT permettre la consultation de la liste des employés | Haute |

### 9.2 Fonctions

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| HR-005 | Le système DOIT permettre la définition de fonctions (EmployeeFunction) | Haute |
| HR-006 | Le système DOIT permettre l'assignation de fonctions aux employés | Haute |

---

## 10. MODULE ACTIONS LÉGALES

### 10.1 Saisies

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| LEG-001 | Le système DOIT permettre l'enregistrement des demandes de saisie (SeizureRequest) | Haute |
| LEG-002 | Le système DOIT tracer les produits concernés | Haute |
| LEG-003 | Le système DOIT bloquer les produits saisis | Haute |

---

## 11. INTÉGRATIONS EXTERNES

### 11.1 Intégration Dynamics AX

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INT-001 | Le système DOIT synchroniser les commandes avec Dynamics AX via SOAP/WCF | Haute |
| INT-002 | Le système DOIT mettre à jour le code AX sur les commandes créées | Haute |
| INT-003 | Le système DOIT gérer les erreurs AX (état: AxError) | Haute |
| INT-004 | Le système DOIT gérer les créations partielles (PartiallyCreatedOnAX) | Moyenne |
| INT-005 | Le système DOIT synchroniser les annulations AX (CanceledAx) | Haute |

### 11.2 Message Broker (RabbitMQ)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INT-006 | Le système DOIT publier les événements sur les queues RabbitMQ | Haute |
| INT-007 | Queue `invent-queue`: messages InventoryMessage, InventoryDecreaseMessage | Haute |
| INT-008 | Queue `preparation-order-queue`: messages DeliveryOrderCompletedCommand | Haute |
| INT-009 | Le système DOIT consommer et traiter les messages entrants | Haute |
| INT-010 | Le système DOIT gérer les retries et dead-letter queues | Moyenne |

### 11.3 Notifications Email (SendGrid)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INT-011 | Le système DOIT envoyer des emails via SendGrid | Haute |
| INT-012 | Le système DOIT notifier les créations de commandes invité | Moyenne |
| INT-013 | Le système DOIT utiliser des templates email (OrderEmailTemplateHelper) | Moyenne |

### 11.4 Cache Redis

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INT-014 | Le système DOIT cacher les commandes en cours d'édition | Haute |
| INT-015 | Le système DOIT cacher les vues InventSum | Haute |
| INT-016 | Le système DOIT gérer les clés: SupplierId + UserId (commandes), ProductId + OrganizationId (stock) | Haute |
| INT-017 | Le système DOIT synchroniser le cache avec la base de données | Haute |

### 11.5 Temps Réel (SignalR)

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| INT-018 | Le système DOIT fournir des hubs SignalR pour les mises à jour temps réel | Haute |
| INT-019 | Hub SalesHub: notifications commandes | Haute |
| INT-020 | Hub InventSumHub: notifications stock | Haute |
| INT-021 | Hub PreparationOrderHub: notifications préparation | Haute |
| INT-022 | Hub QuotaNotification: notifications quotas | Haute |
| INT-023 | Hub ProcurementHub: notifications approvisionnement | Haute |

---

## 12. EXIGENCES NON-FONCTIONNELLES

### 12.1 Sécurité

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-001 | Le système DOIT implémenter OAuth2/OpenID Connect via IdentityServer4 | Haute |
| NFR-002 | Le système DOIT utiliser des tokens JWT Bearer | Haute |
| NFR-003 | Le système DOIT supporter la récupération de token via header et query string | Moyenne |
| NFR-004 | Le système DOIT implémenter l'autorisation basée sur les ressources (PermissionItem, PermissionAction) | Haute |
| NFR-005 | Le système DOIT tracer l'utilisateur courant (ICurrentUser, ICurrentOrganization) | Haute |

### 12.2 Performance

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-006 | Le système DOIT utiliser Redis pour le caching | Haute |
| NFR-007 | Le système DOIT paginer tous les résultats de recherche | Haute |
| NFR-008 | Le système DOIT supporter le traitement asynchrone via message broker | Haute |
| NFR-009 | Le système DOIT optimiser les requêtes avec vues agrégées (InventSum) | Haute |

### 12.3 Validation

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-010 | Le système DOIT utiliser FluentValidation pour la validation des entrées | Haute |
| NFR-011 | Le système DOIT retourner des erreurs de validation détaillées | Haute |
| NFR-012 | Le système DOIT valider au niveau API via ValidateModelStateFilter | Haute |

### 12.4 Logging et Audit

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-013 | Le système DOIT logger via NLog et Serilog | Haute |
| NFR-014 | Le système DOIT tracer: CreatedDateTime, ModifiedDateTime, CreatedByUserId | Haute |
| NFR-015 | Le système DOIT logger les requêtes HTTP (httprequest_log.txt) | Moyenne |
| NFR-016 | Le système DOIT maintenir un event store pour l'audit trail | Moyenne |

### 12.5 Gestion des Erreurs

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-017 | Le système DOIT implémenter HttpGlobalExceptionFilter | Haute |
| NFR-018 | Le système DOIT retourner des réponses d'erreur standardisées | Haute |
| NFR-019 | Le système DOIT gérer les exceptions avec middleware dédié | Haute |

### 12.6 Scalabilité

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-020 | Le système DOIT supporter le déploiement Docker | Moyenne |
| NFR-021 | Le système DOIT supporter les DbContexts multiples par module | Haute |
| NFR-022 | Le système DOIT supporter la communication événementielle inter-modules | Haute |

### 12.7 Documentation

| Réf. | Spécification | Priorité |
|------|---------------|----------|
| NFR-023 | Le système DOIT exposer une documentation Swagger/OpenAPI | Haute |
| NFR-024 | Le système DOIT documenter tous les endpoints REST | Haute |

---

## ANNEXES

### A. Liste des États de Commande

| Code | État | Description |
|------|------|-------------|
| 10 | Pending | En attente |
| 20 | Sent | Envoyée |
| 30 | Accepted | Acceptée |
| 40 | Processing | En traitement |
| 50 | Shipping | En livraison |
| 60 | Completed | Terminée |
| 70 | Canceled | Annulée |
| 80 | Rejected | Rejetée |
| 90 | Prepared | Préparée |
| 100 | Consolidated | Consolidée |
| 110 | InShippingArea | En zone expédition |
| 120 | CreatedOnAx | Créée sur AX |
| 140 | Invoiced | Facturée |
| 150 | BeingWithdrawn | En cours de retrait |
| 160 | Withdrawn | Retirée |
| 170 | AcknowledgmentOfReceipt | Accusé de réception |
| 180 | AxError | Erreur AX |
| 200 | CanceledAx | Annulée AX |
| 210 | PartiallyCreatedOnAX | Partiellement créée AX |

### B. Liste des Types de Transaction Stock

| Type | Description |
|------|-------------|
| Transfer | Transfert inter-zones |
| SupplierReception | Réception fournisseur |
| DeliveryNote | Bon de livraison |
| CustomerReturn | Retour client |

### C. Liste des Motifs de Réclamation (Avoirs)

| Motif | Description |
|-------|-------------|
| Damaged | Produit endommagé |
| NotOrdered | Produit non commandé |
| Expired | Produit périmé |

### D. Endpoints API Principaux

| Module | Endpoint Base | Description |
|--------|---------------|-------------|
| Sales | /api/orders | Gestion des commandes |
| Sales | /api/invoices | Gestion des factures |
| Sales | /api/credit-notes | Gestion des avoirs |
| Sales | /api/discounts | Gestion des remises |
| Inventory | /api/invents | Gestion des stocks |
| Inventory | /api/transactions | Historique mouvements |
| Inventory | /api/batches | Gestion des lots |
| Procurement | /api/supplier-orders | Commandes fournisseurs |
| Procurement | /api/delivery-receipts | Bons de réception |
| Procurement | /api/supplier-invoices | Factures fournisseurs |
| Quota | /api/quotas | Gestion des quotas |
| Quota | /api/quota-requests | Demandes de quota |
| Preparation | /api/preparation-orders | Ordres de préparation |
| Preparation | /api/deleivery-orders | Bons de livraison |
| HR | /api/employees | Gestion employés |

---

**Fin du Cahier des Charges**

*Document généré automatiquement à partir de l'analyse du code source Pharma-Backend*
