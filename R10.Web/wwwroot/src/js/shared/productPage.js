import Image from "../image";
import ActivePage from "../activePage";

export default class ProductPage extends ActivePage {

    constructor() {
        super();
        this.image = new Image();
    }

    onChange_RelatedProduct = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const productName = e.dataItem["ProductName"];
            console.log(productName);

            const grid = $("#prodRelatedProductsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.ProductId = e.dataItem["ProductId"];
            dataItem.ProductName = productName;

            $(row).find(".productName-field").html(productName);

        }
    }

    relatedProductsRowSave = (e) => {
        e.model.RelProductId = this.RelProductId;
        console.log(this.RelProductId);
    }

    onCountryChange = (e) => {
        pageHelper.onComboBoxChangeDisplayName(e, 'CountryName');
    }
}
