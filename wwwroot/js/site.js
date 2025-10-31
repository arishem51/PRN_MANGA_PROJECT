$(() => {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/historyHub")
        .build();

    connection.start()
        .then(() => console.log("✅ SignalR connected to ReadingHistoryHub"))
        .catch(err => console.error(err.toString()));

    // Khi server gửi sự kiện LoadReadingHistory → gọi AJAX load lại trang
    connection.on("LoadReadingHistory", function () {
        console.log("🔁 Đang tải lại lịch sử đọc...");
        LoadReadingHistory();
    });

    // Hàm gọi lại Razor Handler để load phần HTML cập nhật
    function LoadReadingHistory() {
        $.ajax({
            url: '/Public/Manga/ReadingHistory?handler=Reload',
            type: 'GET',
            success: function (response) {
                if (!response.success) {
                    console.warn(response.message);
                    return;
                }

                console.log(response)

                const histories = response.data;
                const container = $('#reading-history-container');

                // Xây lại toàn bộ giao diện như Razor gốc
                let html = `
                <h2 class="mb-3">Lịch sử đọc truyện</h2>
            `;

                if (histories.length === 0) {
                    html += `
                    <div class="alert alert-info">
                        Bạn chưa đọc truyện nào gần đây.
                    </div>
                `;
                } else {
                    html += `
                    <div class="row row-cols-1 row-cols-md-3 g-4">
                `;

                    histories.forEach(history => {
                        html += `
                        <div class="col">
                            <div class="card h-100 shadow-sm">
                                <img src="${history.mangaImage}" class="card-img-top" alt="${history.mangaTitle}" />
                                <div class="card-body">
                                    <h5 class="card-title">${history.mangaTitle}</h5>
                                    <p class="card-text">
                                        Chương cuối đọc: <strong>${history.chapterNumber}</strong><br />
                                        Ngày đọc: ${history.readAt}
                                    </p>
                                    <a href="/Public/Manga/Chapter/${history.chapterId}"
                                       class="btn btn-primary btn-sm">
                                        Tiếp tục đọc
                                    </a>
                                </div>
                            </div>
                        </div>
                    `;
                    });

                    html += `
                    </div>
                `;

                    // Nếu API có thêm totalPages/currentPage, render phân trang như Razor
                    if (response.totalPages && response.totalPages > 1) {
                        html += `
                        <nav class="mt-4">
                            <ul class="pagination justify-content-center">
                    `;
                        for (let i = 1; i <= response.totalPages; i++) {
                            html += `
                            <li class="page-item ${i === response.currentPage ? 'active' : ''}">
                                <a class="page-link" href="/Public/Manga/ReadingHistory?pageNumber=${i}">${i}</a>
                            </li>
                        `;
                        }
                        html += `
                            </ul>
                        </nav>
                    `;
                    }
                }

                // Gắn lại nội dung
                container.html(html);
            },
            error: function (err) {
                console.error("Lỗi khi tải lại lịch sử:", err);
            }
        });
    }

});