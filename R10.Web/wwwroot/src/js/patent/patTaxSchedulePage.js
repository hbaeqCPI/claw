import ActivePage from "../activePage";

export default class PatTaxSchedulePage extends ActivePage {

    updateTaxCost(e) {
        if (e.action === "itemchange" && e.field === "TaxAmount") {
            const model = e.items[0];
            const taxCost = $("#patTaxYearsGrid").find(`tr[data-uid="${model.uid}"] .tax-cost`);
            taxCost.html(model.TaxAmount * model.ExchangeRate);
        }
    }

}



