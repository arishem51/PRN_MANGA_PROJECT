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
        var userId = $("#comment-list-container").data("user-id");

        if (!chapterId) {
            console.error("Không tìm thấy chapterId từ data-chapter-id. Kiểm tra lại file .cshtml.");
            return;
        }
        $.ajax({
            url: `/Public/Manga/Chapter/${chapterId}?handler=Comments&chapterId=${chapterId}&userId=${userId}`,
            method: "GET",
            success: (result) => {
                $("#comment-list-container").html(result);
                setupCommentEvents();
            },
            error: (err) => console.log("Lỗi load comment:", err)
        });
    }

    $(document).on("submit", ".comment-action-form", function (e) {
        e.preventDefault();

        var $form = $(this);
        var url = $form.attr("action");
        var data = $form.serialize();

        // 2. Gửi dữ liệu form bằng AJAX
        $.post(url, data)
            .done(function () {
                $form.find("textarea[name='Input.Content']").val("");
                // Tùy chọn: Ẩn các form reply và edit sau khi gửi
                if ($form.hasClass("reply-form")) { // (Bạn cần thêm class "reply-form" cho form reply)
                    $form.closest(".reply-container").hide();
                }
                if ($form.hasClass("edit-form")) {
                    $form.hide();
                    $form.closest(".media-body").find(".comment-content").show();
                }
            })
            .fail(function () {
                console.error("Có lỗi khi gửi request.");
                alert("Hành động của bạn thất bại, vui lòng thử lại.");
            });
    });

    // Xử lý riêng cho form like/dislike
    $(document).on("submit", "form[asp-page-handler='LikeComment']", function (e) {
        e.preventDefault();
        var $form = $(this);
        var url = $form.attr("action");
        var data = $form.serialize();

        $.post(url, data)
            .done(function (res) {
                console.log("Liked successfully:", res);
                // Không cần gọi LoadCommentData() vì SignalR sẽ tự reload
            })
            .fail(function (err) {
                console.error("Error while liking:", err);
            });
    });



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