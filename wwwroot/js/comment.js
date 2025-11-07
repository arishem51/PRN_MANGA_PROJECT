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

        let openReplyIds = [];
        $("#comment-list-container .reply-list:visible").each(function () {
            openReplyIds.push($(this).attr("id"));
        });

        $.ajax({
            url: `/Public/Manga/Chapter/${chapterId}?handler=Comments&chapterId=${chapterId}&userId=${userId}`,
            method: "GET",
            success: (result) => {
                $("#comment-list-container").html(result);

                if (openReplyIds.length > 0) {
                    openReplyIds.forEach(id => {
                        let replyList = $("#" + id);
                        if (replyList.length) {
                            replyList.show();
                            let commentId = id.replace("reply-list-", "");
                            let button = $(`[data-comment-id='${commentId}'].btn-show-replies`);
                            if (button.length) {
                                button.text("Show less");
                            }
                        }
                    });
                }
            },
            error: (xhr, status, error) => {
                if (xhr.status == 401) {
                    // Bị 401 (chưa đăng nhập / hết hạn)
                    alert("Phiên đăng nhập của bạn đã hết hạn. Vui lòng tải lại trang.");
                    window.location.reload(); // Tải lại trang để đưa về trang login
                } else {
                    console.log("Lỗi load comment:", error);
                }
            }   
        });
    }

    $(document).on("submit", ".comment-action-form", function (e) {
        e.preventDefault();
        var currentUserId = $("#comment-list-container").data("user-id");

        if (!currentUserId || currentUserId === "") {
            window.location.href = "/login"; 
            return;
        }
        var $form = $(this);
        var url = $form.attr("action");
        var data = $form.serialize();

        $.post(url, data)
            .done(function () {
                $form.find("textarea[name='Input.Content']").val("");
                if ($form.hasClass("reply-form")) {
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
            })
            .fail(function (err) {
                console.error("Error while liking:", err);
            });
    });

    setupCommentEvents();

});


function setupCommentEvents() {

    const $container = $("#comment-list-container");

    // 1. Xử lý nút SHOW REPLIES (Giữ nguyên)
    $container.off("click", ".btn-show-replies").on("click", ".btn-show-replies", function () {
        const commentId = $(this).data("comment-id");
        const $replyList = $("#reply-list-" + commentId);
        if (!$replyList.length) return;

        if ($replyList.is(":hidden")) {
            $replyList.show();
            $(this).text("Show less");
        } else {
            $replyList.hide();
            const replyCount = $replyList.children("div").children(".media-block").length;
            $(this).text(`Show more (${replyCount})`);
        }
    });

    // 2. SỬA LỖI NÚT REPLY TẠI ĐÂY
    $container.off("click", ".btn-reply").on("click", ".btn-reply", function () {
        const parentCommentId = $(this).data("comment-id");
        const $mediaBody = $(this).closest(".media-body");

        // Tìm form là con trực tiếp, CÓ class 'comment-action-form'
        // NHƯNG KHÔNG CÓ class 'edit-form'
        const $targetForm = $mediaBody.children("form.comment-action-form:not(.edit-form)");

        // Tìm container bên trong form đó
        const $replyContainer = $targetForm.find(".reply-container");

        if (!$replyContainer.length) {
            console.error("Selector vẫn lỗi! Không tìm thấy reply container.");
            return;
        }

        $replyContainer.find(".parent-id").val(parentCommentId);
        $replyContainer.toggle();
    });

    // 3. Xử lý EDIT/CANCEL (Giữ nguyên)
    $container.off("click", ".btn-edit").on("click", ".btn-edit", function (e) {
        e.preventDefault();
        const $commentBlock = $(this).closest(".media-body");
        $commentBlock.find(".comment-content").hide();
        $commentBlock.find(".edit-form").show();
    });

    $container.off("click", ".btn-cancel-edit").on("click", ".btn-cancel-edit", function () {
        const $commentBlock = $(this).closest(".media-body");
        $commentBlock.find(".edit-form").hide();
        $commentBlock.find(".comment-content").show();
    });
}
