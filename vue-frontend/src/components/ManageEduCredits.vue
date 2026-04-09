<template>
    <div class="manage-credits-page">
        <div class="manage-credits-card">
            <!-- Header / Filters -->
            <div class="top-section">
                <div class="title-row">
                    <div class="title-block">
                        <div class="page-badge">Peer Management</div>
                        <h1>Manage Educational Credits</h1>
                        <p class="page-subtitle">
                            Review continuing education uploads, search by user name, open submitted documents, and mark them as reviewed.
                        </p>
                    </div>

                    <button class="refresh-btn" @click="fetchCredits" :disabled="loading">
                        {{ loading ? "Refreshing..." : "Refresh" }}
                    </button>
                </div>

                <div class="filters-row">
                    <div class="filter-item filter-search">
                        <label>Search User</label>
                        <input v-model="searchText"
                               type="text"
                               placeholder="Search by first name, last name, or full name"
                               @input="handleSearchInput" />
                    </div>

                    <div class="filter-item filter-size">
                        <label>Show Records</label>
                        <select v-model.number="pageSize" @change="onPageSizeChanged">
                            <option :value="10">10</option>
                            <option :value="20">20</option>
                            <option :value="50">50</option>
                            <option :value="100">100</option>
                        </select>
                    </div>
                </div>

                <div class="stats-row">
                    <div class="stat-box">
                        <span>Total Records</span>
                        <strong>{{ totalRecords }}</strong>
                    </div>
                    <div class="stat-box">
                        <span>Page</span>
                        <strong>{{ currentPage }} / {{ totalPages }}</strong>
                    </div>
                    <div class="stat-box">
                        <span>Reviewed On Page</span>
                        <strong>{{ reviewedCount }}</strong>
                    </div>
                </div>
            </div>

            <!-- Table -->
            <div class="table-section">
                <div class="section-header">
                    <h2>Educational Credit List</h2>
                    <p>Showing {{ startRecord }} to {{ endRecord }} of {{ totalRecords }} entries</p>
                </div>

                <div v-if="loading" class="state-box">
                    Loading educational credit documents...
                </div>

                <div v-else-if="errorMessage" class="state-box error-box">
                    {{ errorMessage }}
                </div>

                <div v-else-if="credits.length === 0" class="state-box">
                    No educational credit documents found.
                </div>

                <div v-else class="table-wrap">
                    <table class="credit-table">
                        <thead>
                            <tr>
                                <th>User Name</th>
                                <th>Email</th>
                                <th>File Name</th>
                                <th>No. of Credits</th>
                                <th>Uploaded Date</th>
                                <th>View Document</th>
                                <th>Reviewed</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in credits" :key="row.peerDocSysId">
                                <td>
                                    <div class="name-cell">
                                        <button class="peer-detail-btn" @click="openPeerDetail(row)">
                                            Peer Detail
                                        </button>

                                        <div>
                                            <div class="name-text">{{ row.fullName || "—" }}</div>
                                            <div class="name-subtext">Peer Doc ID: {{ row.peerDocSysId }}</div>
                                        </div>
                                    </div>
                                </td>

                                <td>{{ row.email || "—" }}</td>
                                <td>{{ row.fileName || "Document" }}</td>
                                <td>{{ row.noOfCredits ?? "—" }}</td>
                                <td>{{ formatDate(row.dateUpload) }}</td>

                                <td>
                                    <button class="btn btn-secondary btn-sm" @click="viewDocument(row.peerDocSysId)">
                                        View
                                    </button>
                                </td>

                                <td>
                                    <label class="switch">
                                        <input type="checkbox"
                                               :checked="row.reviewed"
                                               @change="toggleReviewed(row)" />
                                        <span class="slider"></span>
                                    </label>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div v-if="!loading && !errorMessage && credits.length > 0" class="pagination-row">
                    <div class="pagination-info">
                        Showing {{ startRecord }} to {{ endRecord }} of {{ totalRecords }} entries
                    </div>

                    <div class="pagination-controls">
                        <button class="page-btn"
                                @click="goToPage(currentPage - 1)"
                                :disabled="currentPage === 1">
                            Previous
                        </button>

                        <button v-for="page in visiblePages"
                                :key="page"
                                class="page-btn"
                                :class="{ active: page === currentPage, ghost: page === '...' }"
                                :disabled="page === '...'"
                                @click="typeof page === 'number' && goToPage(page)">
                            {{ page }}
                        </button>

                        <button class="page-btn"
                                @click="goToPage(currentPage + 1)"
                                :disabled="currentPage === totalPages">
                            Next
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import apiClient from "@/axios.js";
    let searchDebounceTimer = null;

    export default {
        name: "ManageEduCredits",
        data() {
            return {
                loading: false,
                errorMessage: "",
                credits: [],
                searchText: "",
                currentPage: 1,
                pageSize: 10,
                totalRecords: 0
            };
        },

        computed: {
            totalPages() {
                return Math.max(1, Math.ceil(this.totalRecords / this.pageSize));
            },

            startRecord() {
                if (this.totalRecords === 0) return 0;
                return (this.currentPage - 1) * this.pageSize + 1;
            },

            endRecord() {
                if (this.totalRecords === 0) return 0;
                return Math.min(this.currentPage * this.pageSize, this.totalRecords);
            },

            reviewedCount() {
                return this.credits.filter(x => x.reviewed === true).length;
            },

            visiblePages() {
                const total = this.totalPages;
                const current = this.currentPage;

                if (total <= 7) {
                    return Array.from({ length: total }, (_, i) => i + 1);
                }

                const pages = [1];

                if (current > 3) pages.push("...");

                const start = Math.max(2, current - 1);
                const end = Math.min(total - 1, current + 1);

                for (let i = start; i <= end; i++) {
                    pages.push(i);
                }

                if (current < total - 2) pages.push("...");

                pages.push(total);
                return pages;
            }
        },

        mounted() {
            this.fetchCredits();
        },

        methods: {
            unwrapList(data) {
                if (Array.isArray(data)) return data;
                if (data && Array.isArray(data.$values)) return data.$values;
                return [];
            },
            openPeerDetail(row) {
                this.$router.push(`/peer-management/manage-peer/${row.userId}`);
            },

            async fetchCredits() {
    this.loading = true;
    this.errorMessage = "";

    try {
        const params = new URLSearchParams({
            page: this.currentPage,
            pageSize: this.pageSize
        });

        if (this.searchText && this.searchText.trim()) {
            params.append("search", this.searchText.trim());
        }

        const response = await apiClient.get(
            `/PeerCertification/admin/manage-edu-credits?${params.toString()}`
        );

        const data = response.data;

        this.credits = this.unwrapList(data.items);
        this.totalRecords = data.totalRecords || 0;
        this.currentPage = data.page || 1;
        this.pageSize = data.pageSize || 10;
    } catch (error) {
        this.errorMessage =
            error?.response?.data?.message ||
            error?.response?.data ||
            error?.message ||
            "Unable to load educational credit records.";

        this.credits = [];
        this.totalRecords = 0;
    } finally {
        this.loading = false;
    }
},

            async toggleReviewed(row) {
                const newValue = !row.reviewed;
                const oldValue = row.reviewed;
                row.reviewed = newValue;

                try {
                    const response = await fetch(`/api/PeerCertification/admin/manage-edu-credits/${row.peerDocSysId}/review`, {
                        method: "PUT",
                        
                        body: JSON.stringify({
                            reviewed: newValue
                        })
                    });

                    if (!response.ok) {
                        row.reviewed = oldValue;
                        const message = await response.text();
                        throw new Error(message || "Failed to update review status.");
                    }
                } catch (error) {
                    row.reviewed = oldValue;
                    alert(error?.message || "Unable to update review status.");
                }
            },

            handleSearchInput() {
                this.currentPage = 1;
                clearTimeout(searchDebounceTimer);
                searchDebounceTimer = setTimeout(() => {
                    this.fetchCredits();
                }, 400);
            },

            onPageSizeChanged() {
                this.currentPage = 1;
                this.fetchCredits();
            },

            goToPage(page) {
                if (page < 1 || page > this.totalPages || page === this.currentPage) return;
                this.currentPage = page;
                this.fetchCredits();
            },

            viewDocument(peerDocSysId) {
                window.open(`/api/PeerCertification/uploads/preview/${peerDocSysId}`, "_blank");
            },

            formatDate(value) {
                if (!value) return "—";
                const d = new Date(value);
                if (Number.isNaN(d.getTime())) return "—";
                return d.toLocaleDateString();
            },

            getInitials(name) {
                if (!name) return "U";
                const parts = name.trim().split(/\s+/).filter(Boolean);
                if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
                return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
            }
        }
    };</script>

<style scoped>
    .manage-credits-page {
        padding: 24px;
        background: #f6f8fb;
        min-height: 100%;
    }

    .manage-credits-card {
        display: flex;
        flex-direction: column;
        gap: 22px;
    }

    .top-section,
    .table-section {
        background: #ffffff;
        border: 1px solid #e6eaf0;
        border-radius: 22px;
        box-shadow: 0 10px 28px rgba(15, 23, 42, 0.05);
    }

    .top-section {
        padding: 26px;
    }

    .title-row {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 20px;
        margin-bottom: 24px;
    }

    .page-badge {
        display: inline-flex;
        padding: 8px 14px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #43285d;
        font-size: 12px;
        font-weight: 800;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        margin-bottom: 14px;
    }

    .title-block h1 {
        margin: 0 0 8px;
        font-size: 36px;
        font-weight: 900;
        color: #1f1630;
    }

    .page-subtitle {
        margin: 0;
        font-size: 16px;
        line-height: 1.7;
        color: #667085;
        max-width: 850px;
    }

    .refresh-btn {
        border: none;
        background: #4f2d6f;
        color: white;
        border-radius: 14px;
        padding: 14px 26px;
        font-size: 15px;
        font-weight: 800;
        cursor: pointer;
        min-width: 130px;
        box-shadow: 0 10px 22px rgba(79, 45, 111, 0.22);
    }

        .refresh-btn:disabled {
            opacity: 0.7;
            cursor: not-allowed;
        }

    .filters-row {
        display: grid;
        grid-template-columns: 1.5fr 0.55fr;
        gap: 18px;
        align-items: end;
    }

    .filter-item {
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

        .filter-item label {
            font-size: 14px;
            font-weight: 800;
            color: #374151;
        }

        .filter-item select,
        .filter-item input {
            height: 52px;
            border: 1px solid #d8dee8;
            border-radius: 16px;
            padding: 0 16px;
            font-size: 16px;
            background: #fff;
            color: #111827;
            outline: none;
        }

            .filter-item select:focus,
            .filter-item input:focus {
                border-color: #7c3aed;
                box-shadow: 0 0 0 4px rgba(124, 58, 237, 0.08);
            }

    .stats-row {
        margin-top: 20px;
        display: flex;
        gap: 14px;
        flex-wrap: wrap;
    }

    .stat-box {
        min-width: 180px;
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        padding: 14px 18px;
    }

        .stat-box span {
            display: block;
            font-size: 12px;
            font-weight: 700;
            color: #6b7280;
            text-transform: uppercase;
            margin-bottom: 6px;
            letter-spacing: 0.03em;
        }

        .stat-box strong {
            font-size: 18px;
            color: #111827;
            font-weight: 900;
        }

    .table-section {
        padding: 24px;
    }

    .section-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        margin-bottom: 18px;
    }

        .section-header h2 {
            margin: 0;
            font-size: 24px;
            font-weight: 900;
            color: #1f1630;
        }

        .section-header p {
            margin: 0;
            font-size: 15px;
            color: #6b7280;
        }

    .state-box {
        padding: 26px;
        border-radius: 18px;
        border: 1px solid #e5e7eb;
        background: #f8fafc;
        color: #374151;
        font-size: 15px;
        font-weight: 700;
    }

    .error-box {
        background: #fff7f7;
        color: #991b1b;
        border-color: #fecaca;
    }

    .table-wrap {
        width: 100%;
        overflow-x: auto;
        border: 1px solid #e5e7eb;
        border-radius: 18px;
    }

    .credit-table {
        width: 100%;
        min-width: 1100px;
        border-collapse: collapse;
        background: #fff;
    }

        .credit-table thead th {
            background: #f8fafc;
            color: #374151;
            font-size: 13px;
            font-weight: 900;
            padding: 16px 18px;
            text-align: left;
            border-bottom: 1px solid #e5e7eb;
            white-space: nowrap;
        }

        .credit-table tbody td {
            padding: 16px 18px;
            border-bottom: 1px solid #edf2f7;
            font-size: 14px;
            color: #111827;
            vertical-align: middle;
        }

        .credit-table tbody tr:nth-child(even) {
            background: #fcfcfd;
        }

        .credit-table tbody tr:hover {
            background: #f7f4fb;
        }

    .name-cell {
        display: flex;
        align-items: center;
        gap: 12px;
    }

   

    .name-text {
        font-size: 14px;
        font-weight: 800;
        color: #111827;
    }

    .name-subtext {
        margin-top: 4px;
        font-size: 12px;
        color: #6b7280;
    }

    .btn {
        border: none;
        border-radius: 12px;
        padding: 9px 14px;
        font-size: 13px;
        font-weight: 800;
        cursor: pointer;
    }

    .btn-secondary {
        background: #fff;
        color: #111827;
        border: 1px solid #d1d5db;
    }

        .btn-secondary:hover {
            border-color: #7c3aed;
            color: #43285d;
        }

    .btn-sm {
        padding: 8px 12px;
        font-size: 13px;
    }

    .switch {
        position: relative;
        display: inline-block;
        width: 52px;
        height: 28px;
    }

        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }

    .slider {
        position: absolute;
        inset: 0;
        cursor: pointer;
        background-color: #d1d5db;
        transition: .2s;
        border-radius: 999px;
    }

        .slider:before {
            position: absolute;
            content: "";
            height: 22px;
            width: 22px;
            left: 3px;
            top: 3px;
            background-color: white;
            transition: .2s;
            border-radius: 50%;
            box-shadow: 0 1px 4px rgba(0,0,0,0.18);
        }

    .switch input:checked + .slider {
        background-color: #43285d;
    }

        .switch input:checked + .slider:before {
            transform: translateX(24px);
        }

    .pagination-row {
        margin-top: 18px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
    }

    .pagination-info {
        font-size: 14px;
        color: #6b7280;
    }

    .pagination-controls {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
    }

    .page-btn {
        border: 1px solid #d1d5db;
        background: #fff;
        color: #111827;
        border-radius: 12px;
        min-width: 42px;
        height: 42px;
        padding: 0 14px;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
    }

        .page-btn:hover:not(:disabled) {
            border-color: #7c3aed;
            color: #43285d;
        }

        .page-btn.active {
            background: #43285d;
            color: #fff;
            border-color: #43285d;
        }

        .page-btn.ghost {
            cursor: default;
        }

        .page-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

    @media (max-width: 1100px) {
        .title-row,
        .section-header,
        .pagination-row {
            flex-direction: column;
            align-items: stretch;
        }

        .filters-row {
            grid-template-columns: 1fr;
        }

        .refresh-btn {
            width: 100%;
        }
    }

    @media (max-width: 640px) {
        .manage-credits-page {
            padding: 14px;
        }

        .top-section,
        .table-section {
            padding: 16px;
            border-radius: 18px;
        }

        .title-block h1 {
            font-size: 28px;
        }

        .stats-row {
            flex-direction: column;
        }

        .stat-box {
            width: 100%;
        }
    }
    .peer-detail-btn {
        border: none;
        background: #43285d;
        color: #fff;
        border-radius: 12px;
        padding: 10px 14px;
        font-size: 13px;
        font-weight: 800;
        cursor: pointer;
        white-space: nowrap;
        box-shadow: 0 8px 18px rgba(67, 40, 93, 0.18);
        transition: all 0.18s ease;
    }

        .peer-detail-btn:hover {
            background: #51316f;
            transform: translateY(-1px);
        }
</style>