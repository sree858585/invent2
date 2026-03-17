<template>
    <div class="modal-overlay">
        <div class="modal fade-in">
            <!-- Close -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">
                &times;
            </button>

            <!-- Header -->
            <div class="modal-header-purple">
                <div class="header-left">
                    <div class="header-top-row">
                        <span class="modal-badge">Information</span>

                        <span v-if="banner.startDate || banner.endDate"
                              class="date-pill">
                            {{ formatDateRange(banner.startDate, banner.endDate) }}
                        </span>
                    </div>

                    <h2>{{ banner.modalTitle || banner.bannerName || "Information" }}</h2>

                    <p v-if="banner.bannerName && banner.modalTitle && banner.bannerName !== banner.modalTitle">
                        {{ banner.bannerName }}
                    </p>
                </div>
            </div>

            <!-- Body -->
            <div class="modal-body">

               

                <section class="content-card">
                    <div v-if="banner.modalBodyHtml" class="info-html" v-html="banner.modalBodyHtml"></div>
                    <div class="meta-row" v-if="banner.startDate || banner.endDate">
                        <span v-if="banner.startDate">Available from: {{ formatDate(banner.startDate) }}</span>
                        <span v-if="banner.endDate">Available until: {{ formatDate(banner.endDate) }}</span>
                    </div>
                    <div v-else class="empty-message">
                        No information is available for this banner.
                    </div>
                </section>
            </div>

            <!-- Footer -->
            <div class="modal-footer">
                <button class="btn-primary" type="button" @click="$emit('close')">
                    {{ banner.buttonText || "Close" }}
                </button>
            </div>
        </div>
    </div>
</template>

<script>export default {
        name: "InfoBannerModal",
        props: {
            banner: {
                type: Object,
                required: true
            }
        },
        methods: {
            formatDate(value) {
                if (!value) return "";
                return new Date(value).toLocaleDateString();
            },
            formatDateRange(start, end) {
                const s = this.formatDate(start);
                const e = this.formatDate(end);

                if (s && e) return `${s} - ${e}`;
                if (s) return `Starts ${s}`;
                if (e) return `Until ${e}`;
                return "";
            }
        }
    };</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: radial-gradient(circle at top, rgba(15, 23, 42, 0.24), rgba(15, 23, 42, 0.72));
        backdrop-filter: blur(8px);
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 24px;
        z-index: 9999;
    }

    .modal {
        position: relative;
        width: 920px;
        max-width: 100%;
        max-height: 90vh;
        overflow: hidden;
        display: flex;
        flex-direction: column;
        background: #ffffff;
        border-radius: 28px;
        box-shadow: 0 28px 70px rgba(15, 23, 42, 0.24), 0 0 0 1px rgba(148, 163, 184, 0.16);
        font-family: system-ui, -apple-system, "Segoe UI", sans-serif;
    }

    .close-btn {
        position: absolute;
        top: 16px;
        right: 18px;
        width: 38px;
        height: 38px;
        border: none;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.94);
        color: #2d1f3d;
        font-size: 22px;
        font-weight: 700;
        cursor: pointer;
        z-index: 3;
        box-shadow: 0 6px 18px rgba(15, 23, 42, 0.16);
        transition: all 0.2s ease;
    }

        .close-btn:hover {
            background: #ffffff;
            color: #c62828;
            transform: scale(1.05);
        }

    .modal-header-purple {
        background: linear-gradient(135deg, #43285d 0%, #5b347e 100%);
        padding: 30px 32px 24px;
        color: #ffffff;
    }

    .header-left {
        max-width: calc(100% - 80px);
    }

    .header-top-row {
        display: flex;
        align-items: center;
        gap: 10px;
        flex-wrap: wrap;
        margin-bottom: 12px;
    }

    .modal-badge {
        display: inline-flex;
        align-items: center;
        padding: 7px 14px;
        border-radius: 999px;
        background: rgba(255, 255, 255, 0.14);
        border: 1px solid rgba(255, 255, 255, 0.18);
        color: #ffffff;
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.04em;
        text-transform: uppercase;
    }

    .date-pill {
        display: inline-flex;
        align-items: center;
        padding: 7px 14px;
        border-radius: 999px;
        background: rgba(255, 255, 255, 0.1);
        color: #efe7ff;
        font-size: 12px;
        font-weight: 600;
        border: 1px solid rgba(255, 255, 255, 0.12);
    }

    .modal-header-purple h2 {
        margin: 0;
        font-size: 30px;
        line-height: 1.18;
        font-weight: 800;
        letter-spacing: -0.02em;
    }

    .modal-header-purple p {
        margin: 10px 0 0;
        color: #ddd7f1;
        font-size: 14px;
        line-height: 1.5;
    }

    .modal-body {
        padding: 28px 32px 24px;
        overflow-y: auto;
        background: radial-gradient(circle at top right, rgba(109, 40, 217, 0.04), transparent 25%), linear-gradient(180deg, #fcfcfe 0%, #f8fafc 100%);
    }

    .content-card {
        background: #ffffff;
        border: 1px solid #e7eaf0;
        border-radius: 22px;
        padding: 24px 26px;
        box-shadow: 0 14px 30px rgba(15, 23, 42, 0.05);
    }

    .info-html {
        color: #374151;
        font-size: 15px;
        line-height: 1.85;
    }

        .info-html :deep(h1),
        .info-html :deep(h2),
        .info-html :deep(h3),
        .info-html :deep(h4) {
            color: #1f2937;
            margin: 0 0 12px;
            line-height: 1.3;
        }

        .info-html :deep(p) {
            margin: 0 0 14px;
        }

        .info-html :deep(ul),
        .info-html :deep(ol) {
            margin: 0 0 14px 20px;
            padding-left: 10px;
        }

        .info-html :deep(li) {
            margin-bottom: 8px;
        }

        .info-html :deep(a) {
            color: #43285d;
            font-weight: 600;
            text-decoration: none;
        }

        .info-html :deep(a:hover) {
            text-decoration: underline;
        }

        .info-html :deep(strong) {
            color: #1f2937;
        }

    .empty-message {
        color: #6b7280;
        font-size: 15px;
        line-height: 1.7;
    }

    .modal-footer {
        display: flex;
        justify-content: flex-end;
        padding: 18px 32px 28px;
        background: #ffffff;
        border-top: 1px solid #eef2f7;
    }

    .btn-primary {
        background: #43285d;
        color: #ffffff;
        border: none;
        border-radius: 999px;
        padding: 11px 24px;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        transition: all 0.22s ease;
        box-shadow: 0 8px 18px rgba(67, 40, 93, 0.28);
    }

        .btn-primary:hover {
            background: #361f4a;
            transform: translateY(-2px);
            box-shadow: 0 12px 24px rgba(67, 40, 93, 0.34);
        }

    @media (max-width: 768px) {
        .modal {
            width: 100%;
            max-height: 92vh;
            border-radius: 22px;
        }

        .modal-header-purple {
            padding: 24px 20px 20px;
        }

            .modal-header-purple h2 {
                font-size: 24px;
            }

        .modal-body {
            padding: 18px 20px;
        }

        .content-card {
            padding: 18px;
            border-radius: 18px;
        }

        .modal-footer {
            padding: 16px 20px 20px;
        }

        .btn-primary {
            width: 100%;
        }
    }
    .info-content {
        color: #374151;
        font-size: 15px;
        line-height: 1.8;
    }

        .info-content :deep(h1),
        .info-content :deep(h2),
        .info-content :deep(h3),
        .info-content :deep(h4) {
            color: #2d1f3d;
            margin-top: 0;
            margin-bottom: 14px;
            font-weight: 700;
        }

        .info-content :deep(p) {
            margin: 0 0 14px;
        }

        .info-content :deep(ul),
        .info-content :deep(ol) {
            margin: 0 0 14px 18px;
            padding-left: 10px;
        }

        .info-content :deep(li) {
            margin-bottom: 6px;
        }

        .info-content :deep(a) {
            color: #5b21b6;
            font-weight: 600;
            text-decoration: none;
        }

        .info-content :deep(a:hover) {
            text-decoration: underline;
        }

        .info-content :deep(strong) {
            color: #1f2937;
            font-weight: 700;
        }

        .info-content :deep(blockquote) {
            margin: 16px 0;
            padding: 12px 16px;
            border-left: 4px solid #43285d;
            background: #f8f5fc;
            border-radius: 10px;
            color: #4b5563;
        }

        .info-content :deep(.ql-align-center) {
            text-align: center;
        }

        .info-content :deep(.ql-align-right) {
            text-align: right;
        }

        .info-content :deep(.ql-align-justify) {
            text-align: justify;
        }
</style>