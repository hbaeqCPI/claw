import UserSetupPage from "./userSetupPage";
import UserDetailPage from "./userDetailPage";

if (!window.userSetupPage) {
    window.userSetupPage = new UserSetupPage();
}

if (!window.userDetailPage) {
    window.userDetailPage = new UserDetailPage();
}
