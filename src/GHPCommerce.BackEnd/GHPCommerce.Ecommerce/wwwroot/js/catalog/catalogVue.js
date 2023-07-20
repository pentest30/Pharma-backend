var app = new Vue({
    el: '#catalog-comp',
    data: {
        menus: []
    },
    computed: {
    },
    mounted() {
        axios
            .get('../catalog/GetCatalogs')
            .then(response => (this.menus = response));
    }
});