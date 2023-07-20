var app = new Vue({
    el: '#product-el',
    data: {
        menus: []
    },
    computed: {
    },
    mounted() {
        axios
            .get('../products/GetProductList?catalogId=' + window.catalogId)
            .then(response => (this.menus = response));
    }
});