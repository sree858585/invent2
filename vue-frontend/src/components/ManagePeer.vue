<template>
    <div class="manage-peer-page">
        <div class="manage-peer-card">
            <!-- Top Header + Filters in one section -->
            <div class="top-section">
                <div class="title-row">
                    <div class="title-block">
                        <div class="page-badge">Peer Management</div>
                        <h1>Manage Peer</h1>
                        <p class="page-subtitle">
                            Review peer certification applicants, search by applicant name, and filter by application status.
                        </p>
                    </div>

                    <button class="refresh-btn" @click="fetchPeers" :disabled="loading">
                        {{ loading ? "Refreshing..." : "Refresh" }}
                    </button>
                </div>

                <div class="filters-row">
                    <div class="filter-item filter-view">
                        <label>Applicant View</label>
                        <select v-model="selectedView" @change="onFiltersChanged">
                            <option value="all">View All Applicants</option>
                            <option value="inprogress">View In Progress Applicants</option>
                            <option value="submitted">View Submitted Applicants</option>
                            <option value="approved">View Successfully Approved Applicants</option>
                            <option value="disapproved">View Disapproved Applicants</option>
                            <option value="archived">View Archived Applicants</option>
                            <option value="closed">View Closed Applicants</option>
                            <option value="lapsed">View Lapsed Applicants</option>
                        </select>
                    </div>

                    <div class="filter-item filter-search">
                        <label>Search Applicant</label>
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
                        <span>Current View</span>
                        <strong>{{ selectedViewLabel }}</strong>
                    </div>
                    <div class="stat-box">
                        <span>Page</span>
                        <strong>{{ currentPage }} / {{ totalPages }}</strong>
                    </div>
                </div>
            </div>

            <!-- Table Section -->
            <div class="table-section">
                <div class="section-header">
                    <h2>Applicant List</h2>
                    <p>Showing {{ startRecord }} to {{ endRecord }} of {{ totalRecords }} applicants</p>
                </div>

                <div v-if="loading" class="state-box">
                    Loading applicants...
                </div>

                <div v-else-if="errorMessage" class="state-box error-box">
                    {{ errorMessage }}
                </div>

                <div v-else-if="peers.length === 0" class="state-box">
                    No applicants found for the selected filters.
                </div>

                <div v-else class="table-wrap">
                    <table class="peer-table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Track</th>
                                <th>Status</th>
                                <th>Last Login</th>
                                <th>Last Course Attended</th>
                                <th>Application %</th>
                                <th>Submitted On</th>
                                <th>Last Updated</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="peer in peers"
                                :key="peer.peerSysId"
                                class="clickable-row"
                                @click="openPeerDetail(peer)">
                                <td>
                                    <div class="name-cell">
                                        <div class="name-avatar">
                                            {{ getInitials(peer.fullName) }}
                                        </div>
                                        <div>
                                            <div class="name-text">{{ peer.fullName || "—" }}</div>
                                            <div class="name-subtext">Peer ID: {{ peer.peerSysId }}</div>
                                        </div>
                                    </div>
                                </td>

                                <td>
                                    <div v-if="getTrackList(peer.certificationTrack).length" class="track-pill-wrap">
                                        <span v-for="track in getTrackList(peer.certificationTrack)"
                                              :key="track"
                                              class="track-pill">
                                            {{ track }}
                                        </span>
                                    </div>
                                    <span v-else class="track-pill">—</span>
                                </td>

                                <td>
                                    <span class="status-pill" :class="statusClass(peer.applicationStatus)">
                                        {{ peer.applicationStatus || "Pending" }}
                                    </span>
                                </td>

                                <td>{{ formatDate(peer.lastLoginDate) }}</td>
                                <td>{{ formatDate(peer.lastCourseAttendedDate) }}</td>

                                <td>
                                    <div class="progress-cell">
                                        <div class="progress-label">{{ peer.applicationPercentage ?? 0 }}%</div>
                                        <div class="progress-bar">
                                            <div class="progress-fill"
                                                 :style="{ width: `${peer.applicationPercentage ?? 0}%` }"></div>
                                        </div>
                                    </div>
                                </td>

                                <td>{{ formatDate(peer.submittedOn) }}</td>
                                <td>{{ formatDate(peer.lastUpdated) }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div v-if="!loading && !errorMessage && peers.length > 0" class="pagination-row">
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
    import apiClient from "@/axios";
    let searchDebounceTimer = null;

    export default {
        name: "ManagePeer",
        data() {
            return {
                loading: false,
                errorMessage: "",
                peers: [],
                selectedView: "all",
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

            selectedViewLabel() {
    const map = {
        all: "All Applicants",
        inprogress: "In Progress",
        submitted: "Submitted",
        approved: "Approved",
        disapproved: "Disapproved",
        archived: "Archived",
        closed: "Closed",
        lapsed: "Lapsed"
    };

    return map[this.selectedView] || "All Applicants";
},

            startRecord() {
                if (this.totalRecords === 0) return 0;
                return (this.currentPage - 1) * this.pageSize + 1;
            },

            endRecord() {
                if (this.totalRecords === 0) return 0;
                return Math.min(this.currentPage * this.pageSize, this.totalRecords);
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
            this.fetchPeers();
        },

        methods: {
            unwrapList(data) {
                if (Array.isArray(data)) return data;
                if (data && Array.isArray(data.$values)) return data.$values;
                return [];
            },
            openPeerDetail(peer) {
                this.$router.push(`/peer-management/manage-peer/${peer.userId}`);
            },
            getTrackList(trackValue) {
    if (!trackValue) return [];
    return trackValue
        .split(",")
        .map(x => x.trim())
        .filter(Boolean);
},

            async fetchPeers() {
    this.loading = true;
    this.errorMessage = "";

    try {
        const params = new URLSearchParams({
            view: this.selectedView,
            page: this.currentPage,
            pageSize: this.pageSize
        });

        if (this.searchText && this.searchText.trim()) {
            params.append("search", this.searchText.trim());
        }

        const response = await apiClient.get(
            `/PeerCertification/admin/manage-peer?${params.toString()}`
        );

        const data = response.data;
        const items = this.unwrapList(data.items);

        this.peers = items.map(this.normalizePeerRow);
        this.totalRecords = data.totalRecords || 0;
        this.currentPage = data.page || 1;
        this.pageSize = data.pageSize || 10;

        console.log("manage-peer api response:", data);
        console.log("manage-peer items:", items);
    } catch (error) {
        this.errorMessage =
            error?.response?.data?.message ||
            error?.response?.data ||
            error?.message ||
            "Unable to load applicants.";

        this.peers = [];
        this.totalRecords = 0;
    } finally {
        this.loading = false;
    }
},

            normalizePeerRow(row) {
                return {
                    ...row,
                    lastLoginDate: row.lastLoginDate || null,
                    lastCourseAttendedDate: row.lastCourseAttendedDate || null,
                    applicationPercentage:
                        typeof row.applicationPercentage === "number" ? row.applicationPercentage : 0
                };
            },

            onFiltersChanged() {
                this.currentPage = 1;
                this.fetchPeers();
            },

            onPageSizeChanged() {
                this.currentPage = 1;
                this.fetchPeers();
            },

            handleSearchInput() {
                this.currentPage = 1;
                clearTimeout(searchDebounceTimer);
                searchDebounceTimer = setTimeout(() => {
                    this.fetchPeers();
                }, 400);
            },

            goToPage(page) {
                if (page < 1 || page > this.totalPages || page === this.currentPage) return;
                this.currentPage = page;
                this.fetchPeers();
            },

            formatDate(value) {
                if (!value) return "—";
                const d = new Date(value);
                if (Number.isNaN(d.getTime())) return "—";
                return d.toLocaleDateString();
            },

            statusClass(status) {
    const normalized = (status || "").trim().toLowerCase();

    if (normalized === "disapproved") return "status-disapproved";
    if (normalized === "approved") return "status-approved";
    if (normalized === "archived") return "status-archived";
    if (normalized === "submitted") return "status-submitted";
    if (normalized === "closed") return "status-closed";
    if (normalized === "lapsed") return "status-lapsed";
    if (normalized === "in progress") return "status-pending";

    return "status-pending";
},

            getInitials(name) {
                if (!name) return "P";
                const parts = name.trim().split(/\s+/).filter(Boolean);
                if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
                return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
            }
        }
    };</script>

<style scoped>
    .manage-peer-page {
        padding: 24px;
        background: #f6f8fb;
        min-height: 100%;
    }

    .manage-peer-card {
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
        grid-template-columns: 1.1fr 1.5fr 0.55fr;
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

    .peer-table {
        width: 100%;
        min-width: 1200px;
        border-collapse: collapse;
        background: #fff;
    }

        .peer-table thead th {
            background: #f8fafc;
            color: #374151;
            font-size: 13px;
            font-weight: 900;
            padding: 16px 18px;
            text-align: left;
            border-bottom: 1px solid #e5e7eb;
            white-space: nowrap;
        }

        .peer-table tbody td {
            padding: 16px 18px;
            border-bottom: 1px solid #edf2f7;
            font-size: 14px;
            color: #111827;
            vertical-align: middle;
        }

        .peer-table tbody tr:nth-child(even) {
            background: #fcfcfd;
        }

        .peer-table tbody tr:hover {
            background: #f7f4fb;
        }

    .name-cell {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .name-avatar {
        width: 42px;
        height: 42px;
        min-width: 42px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.1);
        color: #43285d;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 900;
        font-size: 13px;
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

    .track-pill,
    .status-pill {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-height: 32px;
        padding: 6px 12px;
        border-radius: 999px;
        font-size: 12px;
        font-weight: 900;
        white-space: nowrap;
    }

    .track-pill {
        background: #f3f4f6;
        color: #374151;
    }

    .status-approved {
        color: #065f46;
        background: rgba(6, 95, 70, 0.12);
    }

    .status-disapproved {
        color: #991b1b;
        background: rgba(239, 68, 68, 0.14);
    }

    .status-pending {
        color: #92400e;
        background: rgba(245, 158, 11, 0.16);
    }

    .status-submitted {
        color: #1d4ed8;
        background: rgba(59, 130, 246, 0.14);
    }

    .status-archived {
        color: #4b5563;
        background: rgba(107, 114, 128, 0.16);
    }

    .progress-cell {
        min-width: 150px;
    }

    .progress-label {
        font-size: 12px;
        font-weight: 800;
        color: #374151;
        margin-bottom: 8px;
    }

    .progress-bar {
        height: 8px;
        background: #e5e7eb;
        border-radius: 999px;
        overflow: hidden;
    }

    .progress-fill {
        height: 100%;
        border-radius: 999px;
        background: linear-gradient(90deg, #7c3aed 0%, #43285d 100%);
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
        .manage-peer-page {
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
    .clickable-row {
        cursor: pointer;
    }

        .clickable-row:hover {
            background: #f3ecfb !important;
        }
    .track-pill-wrap {
        display: flex;
        flex-wrap: wrap;
        gap: 6px;
    }
    .status-closed {
        color: #374151;
        background: rgba(55, 65, 81, 0.14);
    }

    .status-lapsed {
        color: #9a3412;
        background: rgba(234, 88, 12, 0.14);
    }
</style>