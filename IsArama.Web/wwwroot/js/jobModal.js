document.addEventListener('DOMContentLoaded', function () {
    const jobModal = document.getElementById('jobModal');
    if (!jobModal) return;

    jobModal.addEventListener('show.bs.modal', function (event) {
        const card = event.relatedTarget;
        const jobId = card.getAttribute('data-job-id');

        const modalBody = document.getElementById('modalBody');
        const modalTitle = document.getElementById('modalTitle');
        const modalUrl = document.getElementById('modalOriginalUrl');

        modalTitle.textContent = 'Yükleniyor...';
        modalUrl.href = '#';
        modalBody.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border text-primary"></div>
            </div>`;

        fetch('/Jobs/Detail/' + jobId)
            .then(res => res.text())
            .then(html => {
                modalBody.innerHTML = html;

                const titleEl = modalBody.querySelector('h4');
                if (titleEl) modalTitle.textContent = titleEl.textContent;

                // OriginalUrl'i API'den al
                fetch('/api/jobs/' + jobId, { headers: { 'Accept': 'application/json' } })
                    .catch(() => { });
            })
            .catch(() => {
                modalBody.innerHTML = '<div class="alert alert-danger">İlan yüklenemedi.</div>';
            });

        // OriginalUrl için ayrı istek
        fetch('/Jobs/GetUrl/' + jobId)
            .then(res => res.text())
            .then(url => {
                if (url && url !== 'null') modalUrl.href = url.replace(/"/g, '');
            })
            .catch(() => { });
    });
});
