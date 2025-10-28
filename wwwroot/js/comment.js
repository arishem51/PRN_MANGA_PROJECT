$(() => {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/commentHub")
        .build();

    connection.start()
        .then(() => console.log("SignalR connected"))
        .catch(err => console.error(err.toString()));

    connection.on("LoadComments", LoadCommentData);

    function LoadCommentData() {
        var chapterId = $("#comment-list-container").data("chapter-id");
        if (!chapterId) {
            console.error("Không tìm thấy chapterId từ data-chapter-id. Kiểm tra lại file .cshtml.");
            return;
        }
        $.ajax({
            url: `/Public/Manga/Chapter/${chapterId}?handler=Comments&chapterId=${chapterId}`,
            method: "GET",
            success: (result) => {
                $("#comment-list-container").html(result);
                setupCommentEvents();
            },
            error: (err) => console.log("Lỗi load comment:", err)
        });
    }


});

function setupCommentEvents() {
    document.querySelectorAll(".btn-reply").forEach(button => {
        button.addEventListener("click", function () {
            const parentCommentId = this.dataset.commentId;
            const replyContainer = this.closest(".media-body").querySelector(".reply-container");
            if (!replyContainer) return;

            const hiddenInput = replyContainer.querySelector(".parent-id");
            if (hiddenInput) hiddenInput.value = parentCommentId;

            replyContainer.style.display =
                (replyContainer.style.display == "none" || replyContainer.style.display == "")
                    ? "block"
                    : "none";
        });
    });

    document.querySelectorAll(".btn-edit").forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.preventDefault();
            const commentBlock = btn.closest(".media-body");
            const content = commentBlock.querySelector(".comment-content");
            const editForm = commentBlock.querySelector(".edit-form");

            content.style.display = "none";
            editForm.style.display = "block";

            const cancelBtn = editForm.querySelector(".btn-cancel-edit");
            cancelBtn.addEventListener("click", () => {
                editForm.style.display = "none";
                content.style.display = "block";
            });
        });
    });
}

setupCommentEvents();