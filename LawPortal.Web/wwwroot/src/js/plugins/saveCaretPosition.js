//save textbox/textarea caret position.
//paste method allows text insert at saved position
(function ($) {
    if (typeof $.fn.saveCaretPosition === "undefined") {
        $.fn.saveCaretPosition = function () {
            //default insert at end
            //let pos = this.val().length;

            //use start for consistency with kendo editor
            //default insert at start
            let pos = 0

            this.bind("blur", (e) => {
                pos = e.target.selectionStart;
            });

            this.getPosition = function () {
                return pos;
            }

            //use "paste" for consistency with kendo editor
            this.paste = function (text) {
                const value = this.val().substring(0, pos) + text + this.val().substring(pos);
                this.val(value);

                //set pos after inserted text
                pos = pos + text.length
            }

            return this;
        };
    }
})(jQuery);
