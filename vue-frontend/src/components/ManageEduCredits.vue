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
                        <span>Approved On Page</span>
                        <strong>{{ approvedCount }}</strong>
                    </div>

                    <div class="stat-box">
                        <span>Pending On Page</span>
                        <strong>{{ pendingCount }}</strong>
                    </div>

                    <div class="stat-box">
                        <span>Rejected On Page</span>
                        <strong>{{ rejectedCount }}</strong>
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
                                <th>No. of Documents</th>
                                <th>Total Credits</th>
                                <th>Status Summary</th>
                                <th>Latest Upload</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in credits" :key="row.peerSysId" class="clickable-row" @click="openDocumentsModal(row)">
                                <td>
                                    <div class="name-cell">
                                        <div class="name-avatar">{{ getInitials(row.fullName) }}</div>
                                        <div>
                                            <div class="name-text">{{ row.fullName || "—" }}</div>
                                            <div class="name-subtext">Peer ID: {{ row.peerSysId }}</div>
                                        </div>
                                    </div>
                                </td>

                                <td>{{ row.email || "—" }}</td>
                                <td>{{ row.documentCount }}</td>
                                <td>{{ row.totalCredits ?? "—" }}</td>
                                <td>
                                    <div class="status-summary">
                                        <span class="summary-approved">
                                            {{ row.approvedCount }} Approved
                                        </span>

                                        <span class="summary-pending">
                                            {{ row.pendingCount }} Pending
                                        </span>

                                        <span class="summary-rejected">
                                            {{ row.rejectedCount }} Rejected
                                        </span>
                                    </div>
                                </td>
                                <td>{{ formatDate(row.latestUploadDate) }}</td>

                                <td>
                                    <button class="btn btn-secondary btn-sm" @click.stop="openDocumentsModal(row)">
                                        View Documents
                                    </button>
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
    <div v-if="showDocumentsModal" class="modal-overlay" @click.self="closeDocumentsModal">
        <div class="documents-modal">
            <div class="modal-header">
                <div>
                    <h2>{{ selectedUser?.fullName }}</h2>
                    <p>{{ selectedUser?.email }} • {{ selectedUser?.documentCount }} document(s)</p>
                </div>

                <button class="modal-close" @click="closeDocumentsModal">✕</button>
            </div>

            <div v-if="documentsLoading" class="state-box">
                Loading documents...
            </div>

            <div v-else-if="selectedDocuments.length === 0" class="state-box">
                No documents found.
            </div>

            <div v-else class="table-wrap">
                <table class="credit-table document-edit-table">
                    <thead>
                        <tr>
                            <th>Document Name</th>
                            <th>No. of Credits</th>
                            <th>Uploaded Date</th>
                            <th>Status</th>
                            <th>Admin Comments</th>
                            <th>Document</th>
                            <th>Action</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="doc in selectedDocuments"
                            :key="doc.peerDocSysId">

                            <td>
                                <input v-model.trim="doc.editFileName"
                                       class="table-input file-name-input"
                                       type="text"
                                       maxlength="255"
                                       :disabled="doc.saving" />
                            </td>

                            <td>
                                <input v-model.number="doc.editNoOfCredits"
                                       class="table-input credits-input"
                                       type="number"
                                       min="0"
                                       max="1000"
                                       step="0.25"
                                       :disabled="doc.saving" />
                            </td>

                            <td>
                                {{ formatDate(doc.dateUpload) }}
                            </td>

                            <td>
                                <select v-model.number="doc.editReviewStatus"
                                        class="table-select status-select"
                                        :disabled="doc.saving">

                                    <option v-for="status in reviewStatusOptions"
                                            :key="status.value"
                                            :value="status.value">
                                        {{ status.label }}
                                    </option>
                                </select>
                            </td>

                            <td>
                                <textarea v-model.trim="doc.editAdminComments"
                                          class="table-textarea"
                                          maxlength="2000"
                                          rows="3"
                                          :placeholder="
                            doc.editReviewStatus === 2
                                ? 'Enter the rejection reason'
                                : 'Optional comments for the user'
                        "
                                          :disabled="doc.saving">
                    </textarea>

                                <div v-if="
                            doc.editReviewStatus === 2 &&
                            !doc.editAdminComments
                        "
                                     class="field-hint error-text">
                                    A rejection reason is required.
                                </div>
                            </td>

                            <td>
                                <button class="btn btn-secondary btn-sm"
                                        type="button"
                                        @click="viewDocument(doc.peerDocSysId)">
                                    View
                                </button>
                            </td>

                            <td>
                                <div class="document-actions">
                                    <button class="btn btn-primary btn-sm"
                                            type="button"
                                            :disabled="
                                doc.saving ||
                                !isDocumentValid(doc) ||
                                !hasDocumentChanges(doc)
                            "
                                            @click="saveDocument(doc)">

                                        {{ doc.saving ? "Saving..." : "Save" }}
                                    </button>

                                    <button class="btn btn-secondary btn-sm"
                                            type="button"
                                            :disabled="
                                doc.saving ||
                                !hasDocumentChanges(doc)
                            "
                                            @click="resetDocument(doc)">
                                        Reset
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js";

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
                totalRecords: 0,

                selectedUser: null,
                showDocumentsModal: false,
                documentsLoading: false,
                selectedDocuments: [],

                reviewStatusOptions: [
                    { value: 0, label: "Pending" },
                    { value: 1, label: "Approved" },
                    { value: 2, label: "Rejected" }
                ]
            };
        },

        computed: {
            totalPages() {
                return Math.max(
                    1,
                    Math.ceil(this.totalRecords / this.pageSize)
                );
            },

            startRecord() {
                if (this.totalRecords === 0) return 0;

                return (
                    (this.currentPage - 1) * this.pageSize + 1
                );
            },

            endRecord() {
                if (this.totalRecords === 0) return 0;

                return Math.min(
                    this.currentPage * this.pageSize,
                    this.totalRecords
                );
            },

            approvedCount() {
                return this.credits.reduce(
                    (total, row) =>
                        total + Number(row.approvedCount || 0),
                    0
                );
            },

            pendingCount() {
                return this.credits.reduce(
                    (total, row) =>
                        total + Number(row.pendingCount || 0),
                    0
                );
            },

            rejectedCount() {
                return this.credits.reduce(
                    (total, row) =>
                        total + Number(row.rejectedCount || 0),
                    0
                );
            },

            visiblePages() {
                const total = this.totalPages;
                const current = this.currentPage;

                if (total <= 7) {
                    return Array.from(
                        { length: total },
                        (_, index) => index + 1
                    );
                }

                const pages = [1];

                if (current > 3) {
                    pages.push("...");
                }

                const start = Math.max(2, current - 1);
                const end = Math.min(total - 1, current + 1);

                for (let page = start; page <= end; page++) {
                    pages.push(page);
                }

                if (current < total - 2) {
                    pages.push("...");
                }

                pages.push(total);

                return pages;
            }
        },

        mounted() {
            this.fetchCredits();
        },

        beforeUnmount() {
            clearTimeout(searchDebounceTimer);
        },

        methods: {
            unwrapList(data) {
                if (Array.isArray(data)) {
                    return data;
                }

                if (data && Array.isArray(data.$values)) {
                    return data.$values;
                }

                return [];
            },

            getErrorMessage(error, fallbackMessage) {
                return (
                    error?.response?.data?.message ||
                    error?.response?.data?.title ||
                    error?.response?.data ||
                    error?.message ||
                    fallbackMessage
                );
            },

            prepareDocument(document) {
                return {
                    ...document,

                    editFileName:
                        document.fileName || "Document",

                    editNoOfCredits:
                        document.noOfCredits ?? null,

                    editReviewStatus:
                        Number(document.reviewStatus ?? 0),

                    editAdminComments:
                        document.adminComments || "",

                    originalFileName:
                        document.fileName || "Document",

                    originalNoOfCredits:
                        document.noOfCredits ?? null,

                    originalReviewStatus:
                        Number(document.reviewStatus ?? 0),

                    originalAdminComments:
                        document.adminComments || "",

                    saving: false
                };
            },

            async openDocumentsModal(row) {
                this.selectedUser = row;
                this.showDocumentsModal = true;
                this.selectedDocuments = [];
                this.documentsLoading = true;

                try {
                    await this.loadSelectedDocuments();
                } catch (error) {
                    alert(
                        this.getErrorMessage(
                            error,
                            "Unable to load documents."
                        )
                    );
                } finally {
                    this.documentsLoading = false;
                }
            },

            async loadSelectedDocuments() {
                if (!this.selectedUser?.peerSysId) {
                    return;
                }

                const response = await apiClient.get(
                    `/PeerCertification/admin/manage-edu-credits/` +
                    `${this.selectedUser.peerSysId}/documents`
                );

                this.selectedDocuments = this
                    .unwrapList(response.data)
                    .map(this.prepareDocument);
            },

            closeDocumentsModal() {
                this.showDocumentsModal = false;
                this.selectedUser = null;
                this.selectedDocuments = [];
            },

            async fetchCredits() {
                this.loading = true;
                this.errorMessage = "";

                try {
                    const params = new URLSearchParams({
                        page: String(this.currentPage),
                        pageSize: String(this.pageSize)
                    });

                    const search = this.searchText?.trim();

                    if (search) {
                        params.append("search", search);
                    }

                    const response = await apiClient.get(
                        `/PeerCertification/admin/manage-edu-credits?` +
                        params.toString()
                    );

                    const data = response.data;

                    this.credits = this.unwrapList(data.items);
                    this.totalRecords = Number(data.totalRecords || 0);
                    this.currentPage = Number(data.page || 1);
                    this.pageSize = Number(data.pageSize || 10);
                } catch (error) {
                    this.errorMessage = this.getErrorMessage(
                        error,
                        "Unable to load educational credit records."
                    );

                    this.credits = [];
                    this.totalRecords = 0;
                } finally {
                    this.loading = false;
                }
            },

            isDocumentValid(document) {
                const fileName =
                    document.editFileName?.trim();

                if (!fileName) {
                    return false;
                }

                if (
                    document.editNoOfCredits !== null &&
                    document.editNoOfCredits !== "" &&
                    Number(document.editNoOfCredits) < 0
                ) {
                    return false;
                }

                if (
                    document.editReviewStatus === 2 &&
                    !document.editAdminComments?.trim()
                ) {
                    return false;
                }

                return [0, 1, 2].includes(
                    Number(document.editReviewStatus)
                );
            },

            hasDocumentChanges(document) {
                return (
                    document.editFileName?.trim() !==
                    document.originalFileName?.trim() ||

                    this.normalizeCredits(
                        document.editNoOfCredits
                    ) !==
                    this.normalizeCredits(
                        document.originalNoOfCredits
                    ) ||

                    Number(document.editReviewStatus) !==
                    Number(document.originalReviewStatus) ||

                    (document.editAdminComments || "").trim() !==
                    (document.originalAdminComments || "").trim()
                );
            },

            normalizeCredits(value) {
                if (
                    value === null ||
                    value === undefined ||
                    value === ""
                ) {
                    return null;
                }

                return Number(value);
            },

            async saveDocument(document) {
                if (!this.isDocumentValid(document)) {
                    alert(
                        "Please enter valid document information. " +
                        "A rejection reason is required for rejected documents."
                    );
                    return;
                }

                document.saving = true;

                const payload = {
                    fileName: document.editFileName.trim(),

                    noOfCredits: this.normalizeCredits(
                        document.editNoOfCredits
                    ),

                    reviewStatus: Number(
                        document.editReviewStatus
                    ),

                    adminComments:
                        document.editAdminComments?.trim() || null
                };

                try {
                    const response = await apiClient.put(
                        `/PeerCertification/admin/manage-edu-credits/` +
                        `${document.peerDocSysId}`,
                        payload
                    );

                    const updatedDocument =
                        response.data?.document;

                    if (updatedDocument) {
                        const index =
                            this.selectedDocuments.findIndex(
                                item =>
                                    item.peerDocSysId ===
                                    document.peerDocSysId
                            );

                        if (index >= 0) {
                            this.selectedDocuments[index] =
                                this.prepareDocument(
                                    updatedDocument
                                );
                        }
                    } else {
                        await this.loadSelectedDocuments();
                    }

                    await this.fetchCredits();
                } catch (error) {
                    alert(
                        this.getErrorMessage(
                            error,
                            "Unable to update the document."
                        )
                    );
                } finally {
                    const currentDocument =
                        this.selectedDocuments.find(
                            item =>
                                item.peerDocSysId ===
                                document.peerDocSysId
                        );

                    if (currentDocument) {
                        currentDocument.saving = false;
                    }
                }
            },

            resetDocument(document) {
                document.editFileName =
                    document.originalFileName;

                document.editNoOfCredits =
                    document.originalNoOfCredits;

                document.editReviewStatus =
                    document.originalReviewStatus;

                document.editAdminComments =
                    document.originalAdminComments;
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
                if (
                    typeof page !== "number" ||
                    page < 1 ||
                    page > this.totalPages ||
                    page === this.currentPage
                ) {
                    return;
                }

                this.currentPage = page;
                this.fetchCredits();
            },

            viewDocument(peerDocSysId) {
                window.open(
                    `/api/PeerCertification/uploads/preview/` +
                    peerDocSysId,
                    "_blank",
                    "noopener,noreferrer"
                );
            },

            formatDate(value) {
                if (!value) {
                    return "—";
                }

                const date = new Date(value);

                if (Number.isNaN(date.getTime())) {
                    return "—";
                }

                return date.toLocaleDateString();
            },

            getInitials(name) {
                if (!name) {
                    return "U";
                }

                const parts = name
                    .trim()
                    .split(/\s+/)
                    .filter(Boolean);

                if (parts.length === 1) {
                    return parts[0]
                        .charAt(0)
                        .toUpperCase();
                }

                return (
                    parts[0].charAt(0) +
                    parts[1].charAt(0)
                ).toUpperCase();
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
    .clickable-row {
        cursor: pointer;
    }

        .clickable-row:hover {
            background: #f3ecfb !important;
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

    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.55);
        backdrop-filter: blur(4px);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 3000;
        padding: 24px;
    }

    .documents-modal {
        width: min(1100px, 96vw);
        max-height: 90vh;
        overflow-y: auto;
        background: #ffffff;
        border-radius: 24px;
        padding: 24px;
        box-shadow: 0 30px 80px rgba(15, 23, 42, 0.28);
    }

    .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 16px;
        margin-bottom: 18px;
    }

        .modal-header h2 {
            margin: 0 0 6px;
            font-size: 24px;
            font-weight: 900;
            color: #1f1630;
        }

        .modal-header p {
            margin: 0;
            color: #667085;
            font-size: 15px;
        }

    .modal-close {
        border: none;
        background: #f3f4f6;
        color: #111827;
        width: 40px;
        height: 40px;
        border-radius: 999px;
        cursor: pointer;
        font-weight: 900;
    }
    .btn-primary {
        background: #43285d;
        color: #ffffff;
        border: 1px solid #43285d;
    }

        .btn-primary:hover:not(:disabled) {
            background: #51316f;
        }

    .btn:disabled {
        opacity: 0.55;
        cursor: not-allowed;
    }

    .document-edit-table {
        min-width: 1450px;
    }

    .table-input,
    .table-select,
    .table-textarea {
        width: 100%;
        border: 1px solid #d1d5db;
        border-radius: 10px;
        background: #ffffff;
        color: #111827;
        font-size: 14px;
        outline: none;
    }

    .table-input,
    .table-select {
        min-height: 42px;
        padding: 8px 10px;
    }

    .table-textarea {
        min-width: 250px;
        padding: 10px;
        resize: vertical;
        font-family: inherit;
    }

        .table-input:focus,
        .table-select:focus,
        .table-textarea:focus {
            border-color: #7c3aed;
            box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.08);
        }

    .file-name-input {
        min-width: 260px;
    }

    .credits-input {
        width: 110px;
    }

    .status-select {
        min-width: 130px;
    }

    .document-actions {
        display: flex;
        gap: 8px;
        align-items: center;
    }

    .field-hint {
        margin-top: 6px;
        font-size: 12px;
    }

    .error-text {
        color: #b91c1c;
    }

    .status-summary {
        display: flex;
        flex-direction: column;
        gap: 5px;
        font-size: 12px;
        font-weight: 700;
    }

    .summary-approved {
        color: #166534;
    }

    .summary-pending {
        color: #92400e;
    }

    .summary-rejected {
        color: #991b1b;
    }
</style>