
export default class ActionDelegation {

    constructor() {
        this.newGroupUsers = [];
        this.deletedGroupUsers = [];
    }

    addUser = (user) => {
        this.newGroupUsers.push(user);
    }

    removeUser = (user) => {
        const index = this.newGroupUsers.findIndex(e => e.actId == user.actId && e.ddId == user.ddId && e.id == user.id);
        if (index > -1) {
            this.newGroupUsers.splice(index, 1);
        }
        this.deletedGroupUsers.push(user);
    }

    getNewUsers = () => {
        return this.newGroupUsers;
    }

    getDeletedUsers = () => {
        return this.deletedGroupUsers;
    }

    clearNewUsers = () => {
        this.newGroupUsers = [];
        this.deletedGroupUsers = [];
    }

    processWorkflow = (url) => {
        const newUsers = [];
        const deletedUsers = [];
        const newDelegations = [];
        const deletedDelegations = [];

        //exclude those deleted and then readded
        this.newGroupUsers.forEach(user => {
            const index = this.deletedGroupUsers.findIndex(e => e.actId == user.actId && e.ddId == user.ddId && e.id == user.id && delegationId);
            if (index == -1) {
                newUsers.push(user);
            }
        });
        if (newUsers.length > 0) {
            newUsers.forEach(d => {
                const delegation = {
                    ActId: d.actId == '' ? null : +d.actId,
                    DDId: d.ddId == '' ? null : +d.ddId,
                    UserId: d.id
                };
                newDelegations.push(delegation);
            });
        }

        //exclude those deleted and then readded
        this.deletedGroupUsers.forEach(user => {
            const index = this.newGroupUsers.findIndex(e => e.actId == user.actId && e.ddId == user.ddId && e.id == user.id);
            if (index == -1) {
                deletedUsers.push(user);
            }
        });

        if (deletedUsers.length > 0) {
            deletedUsers.forEach(d => {
                const delegation = {
                    DelegationId: +d.delegationId,
                    ActId: d.actId == '' ? null : +d.actId,
                    DDId: d.ddId == '' ? null : +d.ddId,
                    UserId: d.id
                };
                deletedDelegations.push(delegation);
            });
        }

        if (newDelegations.length > 0 || deletedDelegations.length > 0) {
            $.post(url, { newDelegations, deletedDelegations })
                .done(function (result) {
                    pageHelper.handleEmailWorkflow(result);
                })
                .fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });
        }
    }

    actionDueDelegationIconCheck = (ActId, hasDelegations) => {
        document.querySelectorAll('.delegation-icon-' + ActId).forEach(element => {
            if (hasDelegations) {
                if (element.classList.contains("fa-user")) {
                    element.classList.remove("fa-user");
                    element.classList.add("fa-users");
                }
            } else {
                if (element.classList.contains("fa-users")) {
                    element.classList.remove("fa-users");
                    element.classList.add("fa-user");
                }
            }
        });

        DueDateGridRead();
    }
    

}