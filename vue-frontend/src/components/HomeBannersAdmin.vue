<template>
    <div class="banner-admin-page">
        <div class="page-header">
            <div>
                <h2>Home Page Banners</h2>
                <p>Manage current home page carousel banners.</p>
            </div>

            <button class="add-btn" @click="showCreateModal = true">
                + Add Banner
            </button>
        </div>

        <div class="table-card">
            <div v-if="loading" class="loading-state">Loading banners...</div>

            <div v-else-if="banners.length === 0" class="empty-state">
                No banners found.
            </div>

            <div v-else class="table-wrapper">
                <table class="banner-table">
                    <thead>
                        <tr>
                            <th class="col-preview">Preview</th>
                            <th class="col-name">Banner Name</th>
                            <th class="col-type">Type</th>
                            <th class="col-course">Course ID</th>
                            <th class="col-modal">Modal Title</th>
                            <th class="col-order">Order</th>
                            <th class="col-date">Start Date</th>
                            <th class="col-date">End Date</th>
                            <th class="col-status">Status</th>
                            <th class="col-actions">Actions</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="banner in banners" :key="banner.homeBannerSysId">
                            <td class="col-preview">
                                <div class="preview-box">
                                    <img v-if="banner.imageUrl"
                                         :src="fullImageUrl(banner.imageUrl)"
                                         alt="Banner preview" />
                                    <div v-else class="no-preview">No Image</div>
                                </div>
                            </td>

                            <td class="name-cell col-name" :title="banner.bannerName">
                                {{ banner.bannerName || "-" }}
                            </td>

                            <td class="col-type text-center">
                                <span class="type-badge"
                                      :class="banner.actionType?.toLowerCase()">
                                    {{ banner.actionType || "-" }}
                                </span>
                            </td>

                            <td class="col-course text-center">
                                {{ banner.courseSysId || "-" }}
                            </td>

                            <td class="col-modal modal-cell" :title="banner.modalTitle">
                                {{ banner.modalTitle || "-" }}
                            </td>

                            <td class="col-order text-center">
                                {{ banner.displayOrder }}
                            </td>

                            <td class="col-date text-center">
                                {{ formatDate(banner.startDate) }}
                            </td>

                            <td class="col-date text-center">
                                {{ formatDate(banner.endDate) }}
                            </td>

                            <td class="col-status text-center">
                                <div class="status-wrap">
                                    <label class="switch">
                                        <input type="checkbox"
                                               :checked="banner.active"
                                               @change="toggleActive(banner, $event.target.checked)" />
                                        <span class="slider"></span>
                                    </label>
                                    <div class="status-text" :class="{ inactive: !banner.active }">
                                        {{ banner.active ? "Active" : "Inactive" }}
                                    </div>
                                </div>
                            </td>

                            <td class="col-actions text-center">
                                <button class="edit-btn" @click="openEditModal(banner)">
                                    Edit
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <CreateHomeBannerModal v-if="showCreateModal"
                           @close="showCreateModal = false"
                           @created="handleBannerCreated" />

    <EditHomeBannerModal v-if="showEditModal && selectedBanner"
                         :banner="selectedBanner"
                         @close="showEditModal = false; selectedBanner = null"
                         @updated="handleBannerUpdated" />
</template>

<script>import apiClient from "@/axios";
    import CreateHomeBannerModal from "@/components/Modals/CreateHomeBannerModal.vue"
    import EditHomeBannerModal from "@/components/Modals/EditHomeBannerModal.vue";


    export default {
        name: "HomeBannersAdmin",
        data() {
            return {
                banners: [],
                loading: false,
                showCreateModal: false,
                selectedBanner: null,
                showEditModal: false
            };
        },
        components: {
            CreateHomeBannerModal,
            EditHomeBannerModal
        },
        mounted() {
            this.loadBanners();
        },
        methods: {
            async loadBanners() {
                this.loading = true;
                try {
                    const res = await apiClient.get("/HomeBanner/admin/list");
                    this.banners = Array.isArray(res.data)
                        ? res.data
                        : (res.data?.$values || []);
                } catch (err) {
                    console.error("Failed to load banners:", err);
                    alert("Failed to load home page banners.");
                } finally {
                    this.loading = false;
                }
            },
            openEditModal(banner) {
                this.selectedBanner = { ...banner };
                this.showEditModal = true;
            },

            handleBannerUpdated() {
                this.showEditModal = false;
                this.selectedBanner = null;
                this.loadBanners();
            },
            handleBannerCreated() {
                this.showCreateModal = false;
                this.loadBanners();
            },

            async toggleActive(banner, newValue) {
                const oldValue = banner.active;
                banner.active = newValue;

                try {
                    await apiClient.put(`/HomeBanner/toggle-active/${banner.homeBannerSysId}?active=${newValue}`);
                } catch (err) {
                    banner.active = oldValue;
                    console.error("Failed to toggle banner status:", err);
                    alert(err?.response?.data?.message || "Failed to update banner status.");
                }
            },

            fullImageUrl(url) {
                if (!url) return "";

                if (url.startsWith("http://") || url.startsWith("https://")) {
                    return url;
                }

                const base = apiClient.defaults.baseURL || "";
                const cleanedBase = base.endsWith("/api")
                    ? base.replace(/\/api$/, "")
                    : base;

                return `${cleanedBase}${url}`;
            },

            formatDate(value) {
                if (!value) return "-";
                return new Date(value).toLocaleDateString();
            },
        },
    };</script>

<style scoped>
    .banner-admin-page {
        padding: 28px;
        background: #f6f8fb;
        min-height: 100vh;
    }

    .page-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 16px;
        margin-bottom: 22px;
    }

        .page-header h2 {
            margin: 0;
            font-size: 34px;
            line-height: 1.1;
            color: #2d1f3d;
            font-weight: 800;
        }

        .page-header p {
            margin: 8px 0 0;
            color: #6b7280;
            font-size: 16px;
        }

    .add-btn {
        background: #43285d;
        color: white;
        border: none;
        border-radius: 999px;
        padding: 14px 22px;
        font-weight: 700;
        font-size: 16px;
        cursor: pointer;
        transition: all 0.25s ease;
        box-shadow: 0 6px 16px rgba(67, 40, 93, 0.22);
        white-space: nowrap;
    }

        .add-btn:hover {
            background: #361f4a;
            transform: translateY(-2px);
            box-shadow: 0 10px 22px rgba(67, 40, 93, 0.3);
        }

    .table-card {
        background: white;
        border-radius: 24px;
        border: 1px solid #e5e7eb;
        box-shadow: 0 14px 32px rgba(15, 23, 42, 0.08);
        overflow: hidden;
    }

    .loading-state,
    .empty-state {
        padding: 42px;
        text-align: center;
        color: #6b7280;
        font-size: 15px;
    }

    .table-wrapper {
        overflow-x: auto;
    }

    .banner-table {
        width: 100%;
        border-collapse: separate;
        border-spacing: 0;
        min-width: 1350px;
        table-layout: fixed;
    }

        .banner-table thead tr {
            background: #43285d;
        }

        .banner-table thead th {
            color: #fff;
            font-size: 15px;
            font-weight: 700;
            padding: 18px 16px;
            text-align: left;
            white-space: nowrap;
        }

            .banner-table thead th:first-child {
                border-top-left-radius: 0;
            }

            .banner-table thead th:last-child {
                border-top-right-radius: 0;
            }

        .banner-table tbody td {
            padding: 18px 16px;
            border-bottom: 1px solid #edf0f4;
            vertical-align: middle;
            font-size: 15px;
            color: #1f2937;
            background: #fff;
        }

        .banner-table tbody tr:last-child td {
            border-bottom: none;
        }

        .banner-table tbody tr:hover td {
            background: #fafbff;
        }

    .text-center {
        text-align: center;
    }

    .col-preview {
        width: 150px;
    }

    .col-name {
        width: 230px;
    }

    .col-type {
        width: 110px;
    }

    .col-course {
        width: 110px;
    }

    .col-modal {
        width: 170px;
    }

    .col-order {
        width: 90px;
    }

    .col-date {
        width: 130px;
    }

    .col-status {
        width: 120px;
    }

    .col-actions {
        width: 110px;
    }

    .preview-box {
        width: 138px;
        height: 78px;
        border-radius: 14px;
        overflow: hidden;
        border: 1px solid #e5e7eb;
        background: #f3f4f6;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: inset 0 0 0 1px rgba(255,255,255,0.35);
    }

        .preview-box img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }

    .no-preview {
        font-size: 12px;
        color: #6b7280;
        text-align: center;
    }

    .name-cell {
        font-weight: 700;
        color: #111827;
        word-break: break-word;
    }

    .modal-cell {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .type-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 64px;
        padding: 8px 14px;
        border-radius: 999px;
        font-size: 13px;
        font-weight: 700;
        line-height: 1;
    }

        .type-badge.info {
            background: #ede9fe;
            color: #6d28d9;
        }

        .type-badge.course {
            background: #dcfce7;
            color: #166534;
        }

    .status-wrap {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
    }

    .switch {
        position: relative;
        display: inline-block;
        width: 54px;
        height: 30px;
    }

        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }

    .slider {
        position: absolute;
        inset: 0;
        background-color: #d1d5db;
        border-radius: 999px;
        transition: 0.25s ease;
        cursor: pointer;
    }

        .slider::before {
            content: "";
            position: absolute;
            height: 24px;
            width: 24px;
            left: 3px;
            top: 3px;
            background: white;
            border-radius: 50%;
            transition: 0.25s ease;
            box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
        }

    .switch input:checked + .slider {
        background-color: #22c55e;
    }

        .switch input:checked + .slider::before {
            transform: translateX(24px);
        }

    .status-text {
        font-size: 13px;
        font-weight: 700;
        color: #16a34a;
    }

        .status-text.inactive {
            color: #dc2626;
        }

    .edit-btn {
        background: #eef2ff;
        color: #4338ca;
        border: 1px solid #c7d2fe;
        border-radius: 999px;
        padding: 9px 18px;
        font-size: 13px;
        font-weight: 700;
        cursor: pointer;
        transition: all 0.2s ease;
        min-width: 74px;
    }

        .edit-btn:hover {
            background: #4338ca;
            color: white;
            transform: translateY(-1px);
            box-shadow: 0 8px 18px rgba(67, 56, 202, 0.18);
        }

    @media (max-width: 1024px) {
        .banner-admin-page {
            padding: 18px;
        }

        .page-header {
            flex-direction: column;
            align-items: stretch;
        }

        .add-btn {
            align-self: flex-end;
        }
    }
</style>