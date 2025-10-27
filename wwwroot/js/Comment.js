document.addEventListener("DOMContentLoaded", () => {

    const currentUserIdElement = document.getElementById("current-user-id");
    const currentUserId = currentUserIdElement ? currentUserIdElement.value : null;
    function attachEventListenersToBlock(block) {
        if (!block) return;

        const ownerBlock = block.closest(".comment-block, .reply-block");
        if (ownerBlock !== block) {
            // Ngăn chặn trường hợp block truyền vào không phải là block gốc
            return;
        }

        // 1. Gắn sự kiện SUBMIT cho form Like/Dislike
        block.querySelectorAll(".like-form, .dislike-form").forEach(form => {
            if (form.closest(".comment-block, .reply-block") === block) {
                if (typeof attachLikeDislike === 'function') attachLikeDislike(form);
            }
        });

        // 2. Gắn sự kiện SUBMIT cho form Delete
        block.querySelectorAll(".delete-comment-form").forEach(form => {
            // 🔥 KIỂM TRA:
            if (form.closest(".comment-block, .reply-block") === block) {
                if (typeof attachDelete === 'function') attachDelete(form);
            }
        });

        // 3. Gắn sự kiện SUBMIT cho form Edit
        block.querySelectorAll(".edit-form").forEach(form => {
            // 🔥 KIỂM TRA:
            if (form.closest(".comment-block, .reply-block") === block) {
                if (typeof attachEdit === 'function') attachEdit(form);
            }
        });

        // 4. Gắn sự kiện SUBMIT cho form Reply
        block.querySelectorAll(".reply-form").forEach(form => {
            // 🔥 KIỂM TRA:
            if (form.closest(".comment-block, .reply-block") === block) {
                if (typeof attachReply === 'function') attachReply(form);
            }
        });

        // 5. Gắn sự kiện CLICK cho nút Edit (để hiện form)
        block.querySelectorAll(".btn-edit").forEach(btn => {
            // 🔥 KIỂM TRA:
            if (btn.closest(".comment-block, .reply-block") === block) {
                const contentEl = block.querySelector(".comment-content");
                const editForm = block.querySelector(".edit-form");

                if (contentEl && editForm) {
                    btn.addEventListener("click", (e) => {
                        e.preventDefault();
                        contentEl.style.display = "none";
                        editForm.style.display = "block";
                    });
                }
            }
        });

        // 6. Gắn sự kiện CLICK cho nút Cancel (để ẩn form)
        block.querySelectorAll(".btn-cancel-edit").forEach(btn => {
            // 🔥 KIỂM TRA:
            if (btn.closest(".comment-block, .reply-block") === block) {
                const contentEl = block.querySelector(".comment-content");
                const editForm = block.querySelector(".edit-form");

                if (contentEl && editForm) {
                    btn.addEventListener("click", () => {
                        editForm.style.display = "none";
                        contentEl.style.display = "block";
                    });
                }
            }
        });

        // 7. Gắn sự kiện CLICK cho nút Reply (để hiện/ẩn form reply)
        block.querySelectorAll(".btn-reply").forEach(btn => {
            // 🔥 KIỂM TRA:
            if (btn.closest(".comment-block, .reply-block") === block) {
                btn.addEventListener("click", () => {
                    const replyContainer = block.querySelector(".reply-container");
                    if (replyContainer) {
                        replyContainer.style.display =
                            (replyContainer.style.display === "none" || replyContainer.style.display === "")
                                ? "block"
                                : "none";
                    }
                });
            }
        });
    }
    
    function attachReply(form) {
        form.addEventListener("submit", async e => {
            e.preventDefault();

            const commentId = form.dataset.commentId;
            const chapterId = form.dataset.chapterId;
            const parentId = form.dataset.parentId;
            const mangadexChapterId = form.dataset.mangadexChapterId;
            const content = form.querySelector("textarea").value.trim();
            if (!content) return;

            const response = await fetch(`?handler=Reply`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").value
                },
                body: JSON.stringify({ commentId, content, chapterId, parentId })
            });

            if (!response.ok) {
                console.error("Reply failed:", await response.text());
                return;
            }

            const data = await response.json();
            const isOwner = data.userId === currentUserId; // Sử dụng biến global

            const ownerMenu = isOwner
                ? `
                <div class="dropdown">
                    <button class="btn btn-sm btn-light dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="fa fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                            <a class="dropdown-item btn-edit" href="#" data-comment-id="${data.id}">
                                <i class="fa fa-pencil"></i> Edit
                            </a>
                        </li>
                        <li>
                            <form method="post" action="?handler=DeleteComment" class="delete-comment-form">
                                <input type="hidden" name="Input.Id" value="${data.id}" />
                                <input type="hidden" name="Input.ChapterId" value="${data.chapterId}" />
                                <input type="hidden" name="Input.MangaDexChapterId" value="${data.mangadexChapterId}" />
                                <button type="submit" class="dropdown-item text-danger">
                                    <i class="fa fa-trash"></i> Delete
                                </button>
                            </form>
                        </li>
                    </ul>
                </div>`
                : "";

            const replyHtml = `
            <div class="reply-block">
                <div class="media-block">
                    <a class="media-left" href="#">
                        <img class="img-circle img-sm" src="${data.userAvatarUrl ?? 'https://www.svgrepo.com/show/452030/avatar-default.svg'}" alt="Profile Picture">
                    </a>
                    <div class="media-body">
                        <div class="mar-btm d-flex justify-content-between align-items-start">
                            <div>
                                <a href="#" class="btn-link text-semibold media-heading box-inline">${data.userName}</a>
                                <span class="text-muted text-sm">${data.createdAt}</span>
                            </div>
                            ${ownerMenu}
                        </div>
                        <p class="comment-content">${data.content}</p>
                        <form method="post" class="edit-form" style="display:none;">
                            <textarea name="Input.Content" class="form-control" rows="2">${data.content}</textarea>
                            <input type="hidden" name="Input.Id" value="${data.id}" />
                            <input type="hidden" name="Input.ChapterId" value="${chapterId}" />
                            <input type="hidden" name="Input.MangaDexChapterId" value="${data.mangadexChapterId}" />
                            <div class="mt-2">
                                <button type="submit" class="btn btn-sm btn-primary">Save</button>
                                <button type="button" class="btn btn-sm btn-secondary btn-cancel-edit">Cancel</button>
                            </div>
                        </form>
                        <div class="pad-ver">
                            <div class="btn-group comment-item">
                                <form class="like-form" data-comment-id="${data.id}">
                                    <button type="submit" class="btn btn-sm"><i class="fa fa-thumbs-up"></i> 0</button>
                                </form>
                                <form class="dislike-form" data-comment-id="${data.id}">
                                    <button type="submit" class="btn btn-sm"><i class="fa fa-thumbs-down"></i> 0</button>
                                </form>
                            </div>
                            <button type="button" class="btn btn-sm btn-default btn-hover-primary btn-reply" data-comment-id="${data.id}">Reply</button>
                        </div>
                        <div class="reply-container" style="display:none; margin-top:10px;">
                            <div class="media-block">
                                <a class="media-left" href="#">
                                    <img class="img-circle img-sm" src="${data.userAvatarUrl ?? 'https://www.svgrepo.com/show/452030/avatar-default.svg'}" alt="Profile Picture">
                                </a>
                                <div class="media-body">
                                    <div class="panel">
                                        <div class="panel-body">
                                            <form class="reply-form" data-comment-id="${data.id}" data-chapter-id="${data.chapterId}"
                                                  data-parent-id="${data.parentCommentId}" data-mangadexchapter-id="${data.mangadexChapterId}">
                                                <textarea class="form-control" rows="2" placeholder="What are you thinking?" name="Input.Content"></textarea>
                                                <div class="mar-top clearfix">
                                                    <button class="btn btn-sm btn-primary pull-right" type="submit">
                                                        <i class="fa fa-pencil fa-fw"></i> Reply
                                                    </button>
                                                </div>
                                            </form>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;

            // 🔥 Tìm đúng comment cha để append reply vào
            const commentBlock = form.closest(".comment-block, .reply-block");
            if (!commentBlock) {
                console.error("Không tìm thấy comment-block cha để append reply!");
                return;
            }

            let repliesContainer = commentBlock.querySelector(".replies");
            if (!repliesContainer) {
                repliesContainer = document.createElement("div");
                repliesContainer.classList.add("replies");
                commentBlock.appendChild(repliesContainer);
            }

            repliesContainer.insertAdjacentHTML("afterbegin", replyHtml);

            form.querySelector("textarea").value = "";
            const replyContainer = form.closest(".reply-container");
            if (replyContainer) replyContainer.style.display = "none";

            const newReplyBlock = repliesContainer.firstElementChild;

            // 🔥 DỌN DẸP: Chỉ cần gọi hàm master
            attachEventListenersToBlock(newReplyBlock);
            const total = parseInt(repliesContainer.dataset.total, 10) || 0;
            repliesContainer.dataset.total = total + 1;

            // 2. Tìm nút "Show More"
            // (Dựa trên cấu trúc Razor, nút này là element kế tiếp của .replies)
            const showMoreBtn = repliesContainer.nextElementSibling;

            if (showMoreBtn && showMoreBtn.classList.contains('btn-show-more')) {

                // 3. Cập nhật data-skip (tăng lên 1)
                const skip = parseInt(showMoreBtn.dataset.skip, 10) || 0;
                showMoreBtn.dataset.skip = skip + 1;

                // 4. Cập nhật text của nút (ví dụ: "Show 3 more replies")
                const totalReplies = total + 1;
                const remaining = totalReplies - (skip + 1);

                if (remaining > 0) {
                    showMoreBtn.textContent = `Show ${remaining} ${remaining > 1 ? "more replies" : "more reply"}`;
                } else {
                    // Nếu không còn, có thể ẩn nút đi
                    showMoreBtn.style.display = 'none';
                }
            }
        });
    }

    function attachLikeDislike(form) {
        form.addEventListener("submit", async e => {
            e.preventDefault();
            // ... (Logic like/dislike của bạn giữ nguyên) ...
            const commentId = form.dataset.commentId;
            const reactionType = form.classList.contains("like-form") ? 1 : -1;

            const response = await fetch(`?handler=LikeComment`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").value
                },
                body: JSON.stringify({
                    commentId: commentId,
                    reactionType
                })
            });

            if (response.ok) {
                const data = await response.json();
                const container = form.closest(".comment-item");
                if (container) {
                    const likeButton = container.querySelector(".like-form button");
                    const dislikeButton = container.querySelector(".dislike-form button");

                    if (likeButton)
                        likeButton.innerHTML = `<i class="fa fa-thumbs-up"></i> <strong>${data.likeCount}</strong>`;
                    if (dislikeButton)
                        dislikeButton.innerHTML = `<i class="fa fa-thumbs-down"></i> <strong>${data.dislikeCount}</strong>`;

                    likeButton?.classList.remove("text-primary");
                    dislikeButton?.classList.remove("text-primary");

                    if (data.reactionType === 1) likeButton?.classList.add("text-primary");
                    else if (data.reactionType === -1) dislikeButton?.classList.add("text-primary");
                }
            } else {
                console.error("Like/Dislike request failed:", await response.text());
            }
        });
    }

    function attachDelete(form) {
        form.addEventListener("submit", async (e) => {
            if (form.classList.contains("delete-comment-form")) {
                e.preventDefault();
                // ... (Logic delete của bạn giữ nguyên) ...
                const formData = new FormData(form);
                try {
                    const response = await fetch("?handler=DeleteComment", {
                        method: form.method,
                        headers: {
                            "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']")?.value
                        },
                        body: formData
                    });

                    const data = await response.json();

                    if (data.success) {
                        let commentDiv = form.closest(".reply-block");
                        if (!commentDiv) {
                            commentDiv = form.closest(".panel"); // Giả sử .panel là comment chính
                        }
                        commentDiv?.remove();
                    } else {
                        console.error("Delete failed:", data.message);
                    }
                } catch (err) {
                    console.error("AJAX error:", err);
                }
            }
        });
    }

    function attachEdit(form) {
        form.addEventListener("submit", async (e) => {
            if (form.classList.contains("edit-form")) {
                e.preventDefault();
                // ... (Logic edit của bạn giữ nguyên) ...
                const formData = new FormData(form);
                try {
                    const response = await fetch("?handler=EditComment", {
                        method: "POST",
                        headers: {
                            "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']")?.value
                        },
                        body: formData
                    });
                    const data = await response.json();

                    if (data.success) {
                        const block = form.closest(".comment-block, .reply-block");
                        if (!block) return;

                        const contentP = block.querySelector(".comment-content");
                        contentP.textContent = data.content;

                        form.style.display = "none";
                        contentP.style.display = "block";
                    } else {
                        console.error("Edit failed:", data.message);
                        console.error("Validation Errors:", data.errors);
                    }
                } catch (err) {
                    console.error("AJAX error:", err);
                }
            }
        });
    }

    // =========================================================================
    // XỬ LÝ SUBMIT COMMENT CHÍNH (Gắn listener vào document)
    // =========================================================================

    document.addEventListener("submit", async (e) => {
        const form = e.target;

        if (form.classList.contains("comment-form")) {
            e.preventDefault();
            // ... (Logic submit comment chính của bạn giữ nguyên) ...
            const mangadexchapterId = form.dataset.mangadexchapterId;
            const chapterId = form.dataset.chapterId;
            const content = form.querySelector("textarea").value.trim();

            const response = await fetch("?handler=Comment", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']")?.value
                },
                body: JSON.stringify({
                    ChapterId: chapterId,
                    Content: content,
                    MangaDexChapterId: mangadexchapterId
                })
            });

            if (response.ok) {
                const data = await response.json();
                let dropdownHtml = "";
                if (data.userId === currentUserId) { // Sử dụng biến global
                    dropdownHtml = `
                    <div class="dropdown">
                        <button class="btn btn-sm btn-light dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end">
                            <li>
                                <a class="dropdown-item btn-edit" href="#" data-comment-id="${data.id}">
                                    <i class="fa fa-pencil"></i> Edit
                                </a>
                            </li>
                            <li>
                                <form method="post" action="?handler=DeleteComment" class="delete-comment-form">
                                    <input type="hidden" name="Input.Id" value="${data.id}" />
                                    <input type="hidden" name="Input.ChapterId" value="${chapterId}" />
                                    <input type="hidden" name="Input.MangaDexChapterId" value="${mangadexchapterId}" />
                                    <button type="submit" class="dropdown-item text-danger">
                                        <i class="fa fa-trash"></i> Delete
                                    </button>
                                </form>
                            </li>
                        </ul>
                    </div>`;
                }

                const newCommentHtml = `
                <div class="col-md-12 bootstrap snippets">
                    <div class="panel">
                        <div class="panel-body">
                            <div class="media-block">
                                <a class="media-left" href="#"><img class="img-circle img-sm" src="${data.userAvatarUrl ?? 'https://www.svgrepo.com/show/452030/avatar-default.svg'}" alt="Profile Picture"></a>
                                <div class="media-body comment-block">
                                    <div class="mar-btm d-flex justify-content-between align-items-start">
                                        <div>
                                            <a href="#" class="btn-link text-semibold media-heading box-inline">${data.userName}</a>
                                            <p class="text-muted text-sm">${data.createdAt}</p>
                                        </div>
                                        ${dropdownHtml}
                                    </div>
                                    <p class="comment-content">${data.content}</p>
                                    <form method="post" class="edit-form" style="display:none;">
                                        <textarea name="Input.Content" class="form-control" rows="2">${data.content}</textarea>
                                        <input type="hidden" name="Input.Id" value="${data.id}" />
                                        <input type="hidden" name="Input.ChapterId" value="${chapterId}" />
                                        <input type="hidden" name="Input.MangaDexChapterId" value="${mangadexchapterId}" />
                                        <div class="mt-2">
                                            <button type="submit" class="btn btn-sm btn-primary">Save</button>
                                            <button type="button" class="btn btn-sm btn-secondary btn-cancel-edit">Cancel</button>
                                        </div>
                                    </form>
                                    <div class="pad-ver">
                                        <div class="btn-group comment-item">
                                            <form class="like-form" data-comment-id="${data.id}">
                                                <button type="submit" class="btn btn-sm"><i class="fa fa-thumbs-up"></i> 0</button>
                                            </form>
                                            <form class="dislike-form" data-comment-id="${data.id}">
                                                <button type="submit" class="btn btn-sm"><i class="fa fa-thumbs-down"></i> 0</button>
                                            </form>
                                        </div>
                                        <button type="button" class="btn btn-sm btn-default btn-hover-primary btn-reply" data-comment-id="${data.id}">Reply</button>
                                    </div>
                                    <hr>
                                    <div class="reply-container" style="display:none; margin-top:10px;">
                                        <div class="media-block">
                                            <a class="media-left" href="#"><img class="img-circle img-sm" src="${data.userAvatarUrl ?? 'https://www.svgrepo.com/show/452030/avatar-default.svg'}" alt="Profile Picture"></a>
                                            <div class="media-body">
                                                <div class="mar-btm">
                                                    <a href="#" class="btn-link text-semibold media-heading box-inline">${data.userName}</a>
                                                </div>
                                                <div class="panel">
                                                    <div class="panel-body">
                                                        <form class="reply-form" data-comment-id="${data.id}" data-chapter-id="${chapterId}" data-mangadexchapter-id="${mangadexchapterId}">
                                                            <textarea class="form-control" rows="2" placeholder="What are you thinking?" name="Input.Content"></textarea>
                                                            <div class="mar-top clearfix">
                                                                <button class="btn btn-sm btn-primary pull-right" type="submit"><i class="fa fa-pencil fa-fw"></i> Reply</button>
                                                            </div>
                                                        </form>
                                                    </div>
                                                </div>
                                                <hr />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`;

                const container = document.querySelector(".container.bootdey");
                container.insertAdjacentHTML("afterbegin", newCommentHtml);

                // 🔥 DỌN DẸP: Tìm .comment-block mới và gọi hàm master
                const newCommentBlock = container.firstElementChild.querySelector('.comment-block');
                attachEventListenersToBlock(newCommentBlock);

                form.querySelector("textarea").value = "";
            } else {
                console.error("Comment failed:", await response.text());
            }
        }
    });


    // =========================================================================
    // XỬ LÝ "SHOW MORE" REPLIES
    // =========================================================================

    document.querySelectorAll(".btn-show-more").forEach(button => {
        button.addEventListener("click", async () => {

            // Lấy text của nút *trước khi* thực hiện logic
            const buttonText = button.textContent.trim();
            // Quyết định logic dựa trên text
            const isShowMore = !buttonText.includes("Show Less");

            const parentId = button.dataset.parentId;
            let skip = parseInt(button.dataset.skip, 10);

            if (isShowMore) {
                try {
                    const res = await fetch(`?handler=MoreReplies&parentCommentId=${parentId}&skip=${skip}`);
                    const data = await res.json();
                    const container = document.querySelector(`.replies[data-parent-id='${parentId}']`);

                    data.forEach(reply => {
                        const isOwner = reply.userId === currentUserId;
                        const createdAtFormatted = new Date(reply.createdAt).toLocaleString();
                        const userAvatarUrl = reply.userAvatarUrl ?? 'https://www.svgrepo.com/show/452030/avatar-default.svg';
                        const likesCount = reply.likes.filter(r => r.reactionType === 1).length;
                        const dislikesCount = reply.likes.filter(r => r.reactionType === -1).length;
                        const chapterId = reply.chapterId || '';
                        const mangadexChapterId = reply.mangaDexChapterId || '';
                        const parentId = reply.parentCommentId; // Chú ý: tên biến này trùng với parentId ở ngoài

                        const ownerMenu = isOwner
                            ? `
                            <div class="dropdown">
                                <button class="btn btn-sm btn-light dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fa fa-ellipsis-v"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li>
                                        <a class="dropdown-item btn-edit" href="#" data-comment-id="${reply.id}">
                                            <i class="fa fa-pencil"></i> Edit
                                        </a>
                                    </li>
                                    <li>
                                        <form method="post" action="?handler=DeleteComment" class="delete-comment-form">
                                            <input type="hidden" name="Input.Id" value="${reply.id}" />
                                            <input type="hidden" name="Input.ChapterId" value="${chapterId}" />
                                            <input type="hidden" name="Input.MangaDexChapterId" value="${mangadexChapterId}" />
                                            <button type="submit" class="dropdown-item text-danger">
                                                <i class="fa fa-trash"></i> Delete
                                            </button>
                                        </form>
                                    </li>
                                </ul>
                            </div>`
                            : "";

                        const html = `
                        <div class="reply-block" data-reply-id="${reply.id}">
                            <div class="media-block">
                                <a class="media-left" href="#">
                                    <img class="img-circle img-sm" src="${userAvatarUrl}" alt="Profile Picture">
                                </a>
                                <div class="media-body">
                                    <div class="mar-btm d-flex justify-content-between align-items-start">
                                        <div>
                                            <a href="#" class="btn-link text-semibold media-heading box-inline">${reply.userName}</a>
                                            <span class="text-muted text-sm">${createdAtFormatted}</span>
                                        </div>
                                        ${ownerMenu}
                                    </div>
                                    <p class="comment-content">${reply.content}</p>
                                    ${isOwner ? `
                                    <form method="post" class="edit-form" style="display:none;">
                                        <textarea name="Input.Content" class="form-control" rows="2">${reply.content}</textarea>
                                        <input type="hidden" name="Input.Id" value="${reply.id}" />
                                        <input type="hidden" name="Input.ChapterId" value="${chapterId}" />
                                        <input type="hidden" name="Input.MangaDexChapterId" value="${mangadexChapterId}" />
                                        <div class="mt-2">
                                            <button type="submit" class="btn btn-sm btn-primary">Save</button>
                                            <button type="button" class="btn btn-sm btn-secondary btn-cancel-edit">Cancel</button>
                                        </div>
                                    </form>
                                    ` : ''}
                                    <div class="pad-ver">
                                        <div class="btn-group comment-item">
                                            <form class="like-form" data-comment-id="${reply.id}">
                                                <button type="submit" class="btn btn-sm"><i class="fa fa-thumbs-up"></i> ${likesCount}</button>
                                            </form>
                                            <form class="dislike-form" data-comment-id="${reply.id}">
                                                <button type="submit" class="btn btn-sm"><i class="fa fa-thumbs-down"></i> ${dislikesCount}</button>
                                            </form>
                                        </div>
                                        <button type="button" class="btn btn-sm btn-default btn-hover-primary btn-reply" data-comment-id="${reply.id}">Reply</button>
                                    </div>
                                    <div class="reply-container" style="display:none; margin-top:10px;">
                                        <div class="media-block">
                                            <a class="media-left" href="#">
                                                <img class="img-circle img-sm" src="${userAvatarUrl}" alt="Profile Picture">
                                            </a>
                                            <div class="media-body">
                                                <div class="panel">
                                                    <div class="panel-body">
                                                        <form class="reply-form" data-comment-id="${reply.id}" data-chapter-id="${reply.chapterId}"
                                                              data-parent-id="${parentId}" data-mangadexchapter-id="${mangadexChapterId}">
                                                            <textarea class="form-control" rows="2" placeholder="What are you thinking?" name="Input.Content"></textarea>
                                                            <div class="mar-top clearfix">
                                                                <button class="btn btn-sm btn-primary pull-right" type="submit">
                                                                    <i class="fa fa-pencil fa-fw"></i> Reply
                                                                </button>
                                                            </div>
                                                        </form>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>`;

                        container.insertAdjacentHTML("beforeend", html);

                        // 🔥 DỌN DẸP: GẮN SỰ KIỆN CHO COMMENT MỚI
                        const newReplyBlock = container.lastElementChild;
                        attachEventListenersToBlock(newReplyBlock);

                    }); // Kết thúc data.forEach

                    // Cập nhật trạng thái nút Show More/Show Less
                    skip += data.length;
                    button.dataset.skip = skip;
                    const totalReplies = parseInt(container.dataset.total, 10);
                    if (skip >= totalReplies) {
                        button.textContent = "Show Less";
                        button.dataset.skip = totalReplies;
                    }

                } catch (err) {
                    console.error("Error loading replies:", err);
                }

            } else {
                // --- Xử lý SHOW LESS ---
                const container = document.querySelector(`.replies[data-parent-id='${parentId}']`);
                const allReplies = Array.from(container.querySelectorAll(".reply-block"));

                // (Logic này của bạn là giữ lại 2, tôi giữ nguyên)
                const repliesToShow = allReplies.slice(0, 2);
                container.innerHTML = repliesToShow.map(el => el.outerHTML).join('');

                // 🔥 DỌN DẸP: GẮN LẠI SỰ KIỆN CHO 2 REPLY VỪA TẠO LẠI
                container.querySelectorAll(".reply-block").forEach(recreatedReplyBlock => {
                    attachEventListenersToBlock(recreatedReplyBlock);
                });

                // (Giữ logic cũ của bạn)
                const totalReplies = parseInt(container.dataset.total, 10);
                const remaining = totalReplies - 2;
                button.textContent = `Show ${remaining > 0 ? remaining + ' more' : ''} replies`;
                button.dataset.skip = 2;
            }
        });
    });

    document.querySelectorAll(".comment-block, .reply-block").forEach(existingBlock => {
        attachEventListenersToBlock(existingBlock);
    });

});