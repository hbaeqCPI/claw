import UserSetupPage from "./userSetupPage";
import UserDetailPage from "./userDetailPage";
import TaskSchedulerPage from "./taskSchedulerPage";

if (!window.userSetupPage) {
    window.userSetupPage = new UserSetupPage();
}

if (!window.userDetailPage) {
    window.userDetailPage = new UserDetailPage();
}

if (!window.taskSchedulerPage) {
    window.taskSchedulerPage = new TaskSchedulerPage();
}
