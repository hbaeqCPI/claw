import ActivePage from "../activePage";

export default class RMSReminderSetupPage extends ActivePage {

    constructor() {
        super();
    }

    onChangeCountry = (e) => {
        const comboBox = this.getKendoComboBox("CaseType");
        comboBox.element.data("fetched", 0);
        pageHelper.onComboBoxChangeDisplayName(e, 'CountryName');
    }

    onChangeCaseType = (e) => {
        const comboBox = this.getKendoComboBox("Country");
        comboBox.element.data("fetched", 0);
        pageHelper.onComboBoxChangeDisplayName(e, 'Description');
    }

    getCountry = () => {
        const comboBox = this.getKendoComboBox("Country");
        return { country: comboBox.value() };
    }
}