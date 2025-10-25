function attachReply(form) {
    form.addEventListener("submit", async e => {
        e.preventDefault();

        const commentId = form.dataset.commentId;
        const chapterId = form.dataset.chapterId;
        const mangadexChapterId = form.dataset.mangadexChapterId;
        const content = form.querySelector("textarea").value.trim();
        if (!content) return;

        const response = await fetch(`?handler=Reply`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").value
            },
            body: JSON.stringify({ commentId, content, chapterId })
        });

        if (!response.ok) {
            console.error("Reply failed:", await response.text());
            return;
        }

        const data = await response.json();
        console.log(data)
        const isOwner = data.userId === data.currentUserId;

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
                        <input type="hidden" name="Input.MangaDexChapterId" value="${data.mangadexchapterId}" />
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
								<form class="reply-form" data-comment-id="${data.id}" data-chapter-id="${data.chapterId}" data-mangadexchapter-id="${data.mangadexChapterId}">
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
</div>
`;


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

        newReplyBlock.querySelectorAll(".like-form, .dislike-form").forEach(f => attachLikeDislike(f));
        newReplyBlock.querySelectorAll(".delete-comment-form").forEach(f => attachDelete(f));

        // edit attach
        const contentEl = newReplyBlock.querySelector(".comment-content");
        const editForm = newReplyBlock.querySelector(".edit-form");
        const editBtn = newReplyBlock.querySelector(".btn-edit");
        const cancelBtn = newReplyBlock.querySelector(".btn-cancel-edit");

        // Khi nhấn "Edit" trong dropdown
        if (editBtn && editForm && contentEl) {
            editBtn.addEventListener("click", (e) => {
                e.preventDefault();
                contentEl.style.display = "none";
                editForm.style.display = "block";
            });
        }

        // Khi nhấn "Cancel"
        if (cancelBtn && editForm && contentEl) {
            cancelBtn.addEventListener("click", () => {
                editForm.style.display = "none";
                contentEl.style.display = "block";
            });
        }

        // Gắn logic edit form (nếu có hàm attachEdit)
        const newEditForms = newReplyBlock.querySelectorAll(".edit-form");
        newEditForms.forEach(f => attachEdit(f));

        //reply attach
        newReplyBlock.querySelectorAll(".reply-form").forEach(f => attachReply(f));

        const replyBtn = newReplyBlock.querySelector(".btn-reply");
        if (replyBtn) {
            replyBtn.addEventListener("click", () => {
                const replyContainer = newReplyBlock.querySelector(".reply-container");
                if (!replyContainer) return;
                replyContainer.style.display =
                    replyContainer.style.display === "none" || replyContainer.style.display === ""
                        ? "block"
                        : "none";
            });
        }
    });
}

// Khởi tạo attach cho tất cả reply form ban đầu
document.querySelectorAll(".reply-form").forEach(f => attachReply(f));


function attachLikeDislike(form) {
    form.addEventListener("submit", async e => {
        e.preventDefault();

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

            if (!container) {
                console.warn("Cannot find container with .comment-item for commentId:", commentId);
            }

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
            const errText = await response.text();
            console.error("Like/Dislike request failed:", errText);
        }
    });
}


document.querySelectorAll(".like-form, .dislike-form").forEach(f => attachLikeDislike(f));

function attachDelete(form) {
    form.addEventListener("submit", async (e) => {

        if (form.classList.contains("delete-comment-form")) {
            e.preventDefault();

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
                        commentDiv = form.closest(".panel");
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

document.querySelectorAll(".delete-comment-form").forEach(f => attachDelete(f));

function attachEdit(form) {
    form.addEventListener("submit", async (e) => {

        if (form.classList.contains("edit-form")) {
            e.preventDefault();

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

                    // In toàn bộ object data ra console để thấy mảng errors
                    console.error("Validation Errors:", data.errors);
                }
            } catch (err) {
                console.error("AJAX error:", err);
            }
        }
    });
}

document.querySelectorAll(".edit-form").forEach(f => attachEdit(f));


document.addEventListener("submit", async (e) => {
    const form = e.target;

    if (form.classList.contains("comment-form")) {
        e.preventDefault();
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
            console.log(data)
            let dropdownHtml = "";
            if (data.userId === data.currentUserId) {
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
                <form method="post" asp-page="Chapter" asp-page-handler="DeleteComment" class="delete-comment-form">
                    <input type="hidden" name="Input.Id" value="${data.id}" />
                    <input type="hidden" name="Input.ChapterId" value="${chapterId}" />
                    <input type="hidden" name="Input.MangaDexChapterId" value="${mangadexchapterId}" />
                    <button type="submit" class="dropdown-item text-danger">
                        <i class="fa fa-trash"></i> Delete
                    </button>
                </form>
            </li>
        </ul>
    </div>
    `;
            }

            const newCommentHtml = `
<div class="col-md-12 bootstrap snippets">
    <div class="panel">
        <div class="panel-body">
            <div class="media-block">
                <a class="media-left" href="#"><img class="img-circle img-sm" src="https://www.svgrepo.com/show/452030/avatar-default.svg" alt="Profile Picture"></a>
                <div class="media-body comment-block">
                    <div class="mar-btm">
                        <a href="#" class="btn-link text-semibold media-heading box-inline">${data.userName}</a>
                        <p class="text-muted text-sm">${data.createdAt}</p>
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
                                <button type="submit" class="btn btn-sm">
                                    <i class="fa fa-thumbs-up"></i> 0
                                </button>
                            </form>

                            <form class="dislike-form" data-comment-id="${data.id}">
                                <button type="submit" class="btn btn-sm">
                                    <i class="fa fa-thumbs-down"></i> 0
                                </button>
                            </form>
                        </div>

                        <button type="button" class="btn btn-sm btn-default btn-hover-primary btn-reply"
                            data-comment-id="${data.id}">
                            Reply
                        </button>
                    </div>

                    <hr>

                    <div class="reply-container" style="display:none; margin-top:10px;">
                        <div class="media-block">
                            <a class="media-left" href="#"><img class="img-circle img-sm" src="https://www.svgrepo.com/show/452030/avatar-default.svg" alt="Profile Picture"></a>
                            <div class="media-body">
                                <div class="mar-btm">
                                    <a href="#" class="btn-link text-semibold media-heading box-inline">${data.userName}</a>
                                </div>
                                <div class="panel">
                                    <div class="panel-body">
                                        <form class="reply-form" data-comment-id="${data.id}" data-chapter-id="${chapterId}">
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
</div>
            `;


            // Chèn comment mới vào đầu danh sách
            const container = document.querySelector(".container.bootdey");
            container.insertAdjacentHTML("afterbegin", newCommentHtml);
            const newCommentForms = container.querySelectorAll(
                `.like-form[data-comment-id='${data.id}'], 
     .dislike-form[data-comment-id='${data.id}']`
            );

            newCommentForms.forEach(f => attachLikeDislike(f));

            const newDeleteForms = container.querySelectorAll(".delete-comment-form");
            newDeleteForms.forEach(f => attachDelete(f));

            const newCommentBlock = container.querySelector(".comment-block");

            const newEditBtn = newCommentBlock.querySelector(".btn-edit");
            const editForm = newCommentBlock.querySelector(".edit-form");
            const content = newCommentBlock.querySelector(".comment-content");

            if (newEditBtn) {
                newEditBtn.addEventListener("click", (e) => {
                    e.preventDefault();
                    content.style.display = "none";
                    editForm.style.display = "block";
                });
            }

            const cancelBtn = newCommentBlock.querySelector(".btn-cancel-edit");
            if (cancelBtn) {
                cancelBtn.addEventListener("click", () => {
                    editForm.style.display = "none";
                    content.style.display = "block";
                });
            }

            const newEditForms = container.querySelectorAll(".edit-form");
            newEditForms.forEach(f => attachEdit(f));

            const newReplyButton = newCommentBlock.querySelector(".btn-reply");
            if (newReplyButton) {
                newReplyButton.addEventListener("click", () => {
                    const container = newReplyButton.closest(".reply-block, .panel-body");
                    const replyContainer = container.querySelector(".reply-container");
                    if (!replyContainer) {
                        console.warn("Không tìm thấy reply-container");
                        return;
                    }

                    replyContainer.style.display =
                        (replyContainer.style.display === "none" || replyContainer.style.display === "")
                            ? "block"
                            : "none";
                });
            }

            const newReply = container.querySelectorAll(".reply-form");
            newReply.forEach(f => attachReply(f));

            // Xóa nội dung textarea
            form.querySelector("textarea").value = "";
        } else {
            console.error("Comment failed:", await response.text());
        }
    }
});












