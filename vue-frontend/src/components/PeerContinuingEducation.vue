<template>
    <div class="ce-page">
        <div class="ce-card">
            <div class="ce-header">
                <div class="ce-header">
                    <button class="back-btn" @click="goBackToPeerCertification">
                        <span class="back-arrow">←</span>
                        Back to Peer Certification
                    </button>

                    <div>
                        <div class="ce-badge">Certified Peer Worker Portal</div>
                        <h1>Continuing Education Credits</h1>
                        <p class="ce-subtitle">
                            Maintain your peer worker certification by submitting continuing education documentation.
                        </p>
                    </div>
                </div>
                
            </div>

            <div v-if="loading" class="state-card">
                Loading continuing education details...
            </div>

            <div v-else-if="errorMessage" class="state-card error-card">
                {{ errorMessage }}
            </div>

            <template v-else>
                <section class="section">
                    <div class="section-title-row">
                        <h2>Your Certified Tracks</h2>
                        <span class="section-chip">{{ certificationTracks.length }} Track<span v-if="certificationTracks.length !== 1">s</span></span>
                    </div>

                    <div v-if="certificationTracks.length" class="track-grid">
                        <div v-for="track in certificationTracks"
                             :key="track.code"
                             class="track-card">
                            <div class="track-top">
                                <div class="track-code">{{ track.code }}</div>
                                <div class="track-pill">Certified</div>
                            </div>

                            <div class="track-label">{{ trackLabel(track.code) }}</div>

                            <div class="track-date">
                                Certification Date
                                <strong>{{ formatDate(track.certDate) }}</strong>
                            </div>
                        </div>
                    </div>

                    <div v-else class="muted-box">
                        No approved certification tracks found.
                    </div>
                </section>

                <section class="section">
                    <h2>AIDS Institute Peer Certification Continuing Education</h2>

                    <div class="content-card">
                        <p>
                            In order to maintain certification, AI Certified Peer Workers must complete a minimum of
                            10 hours of training, or Continuing Education Credits (CEUs) per year.
                            This requirement must be submitted every 2 years, prior to your anniversary date of certification,
                            through your online application.
                        </p>

                        <p>
                            The training you receive towards your continuing education should be directly related to
                            HIV, HCV, Harm Reduction, PrEP, and/or Criminal Justice, and should improve and strengthen
                            your ability to provide services.
                        </p>

                        <p>
                            In order to receive credit, training must be obtained through one of the following:
                        </p>

                        <ul class="ce-list">
                            <li>Courses from the AI Peer Worker Certification Course Catalogue via your hivtrainingny.org account</li>
                            <li>TTAP (with certificate)</li>
                            <li>AETC (with certificate)</li>
                            <li>Your employer (with certificate)</li>
                        </ul>

                        <div class="important-note">
                            Please be sure to review your contact information in your user profile and make any necessary updates.
                        </div>
                    </div>
                </section>

                <section class="section">
                    <h2>Upload Additional Documents</h2>

                    <div class="upload-card">
                        <div class="upload-form">
                            <div class="field">
                                <label>No. of Credits</label>
                                <input v-model="credits"
                                       type="number"
                                       min="0"
                                       step="0.25"
                                       placeholder="Enter credits earned" />
                            </div>

                            <div class="field file-field">
                                <label>Upload Document</label>

                                <input ref="fileInput"
                                       class="hidden-file-input"
                                       type="file"
                                       @change="handleFileSelect"
                                       :disabled="uploading" />

                                <div class="file-picker-row">
                                    <button type="button"
                                            class="file-picker-btn"
                                            @click="$refs.fileInput.click()"
                                            :disabled="uploading">
                                        Choose File
                                    </button>

                                    <div class="file-name-box">
                                        {{ selectedFile ? selectedFile.name : "No file selected" }}
                                    </div>
                                </div>
                            </div>

                            <div class="actions">
                                <button class="btn btn-primary"
                                        @click="uploadDocument"
                                        :disabled="uploading || !selectedFile">
                                    {{ uploading ? "Uploading..." : "Upload" }}
                                </button>
                            </div>
                        </div>

                        <div v-if="uploadMessage"
                             class="upload-message"
                             :class="{ success: uploadMessage.toLowerCase().includes('success'), error: !uploadMessage.toLowerCase().includes('success') }">
                            {{ uploadMessage }}
                        </div>
                    </div>
                </section>

                <section class="section">
                    <div class="section-title-row">
                        <h2>Submitted CE Documents</h2>
                        <span class="section-chip">{{ ceDocs.length }} Document<span v-if="ceDocs.length !== 1">s</span></span>
                    </div>

                    <div v-if="ceDocs.length === 0" class="muted-box">
                        No continuing education documents uploaded yet.
                    </div>

                    <div v-else class="table-wrap">
                        <table class="ce-table">
                            <thead>
                                <tr>
                                    <th>File Name</th>
                                    <th>No. of Credits</th>
                                    <th>Date Uploaded</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="doc in ceDocs" :key="doc.peerDocSysId">
                                    <td class="file-name-cell">{{ doc.fileName || "Document" }}</td>
                                    <td>{{ doc.noOfCredits ?? "—" }}</td>
                                    <td>{{ formatDate(doc.dateUpload) }}</td>
                                    <td>
                                        <div class="row-actions">
                                            <button class="btn btn-secondary btn-sm" @click="downloadDoc(doc.peerDocSysId)">
                                                View
                                            </button>
                                            <button class="btn btn-danger btn-sm" @click="deleteDoc(doc.peerDocSysId)">
                                                Remove
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </section>
            </template>
        </div>
    </div>
</template>

<script>export default {
        name: "PeerContinuingEducation",
        data() {
            return {
                loading: false,
                uploading: false,
                errorMessage: "",
                uploadMessage: "",
                certificationTracks: [],
                ceDocs: [],
                credits: "",
                selectedFile: null
            };
        },

        async mounted() {
            await this.loadPage();
        },

        methods: {
            getUserGuid() {
                return localStorage.getItem("userId");
            },
            goBackToPeerCertification() {
    this.$router.push("/peer-certification");
},

            async apiFetch(url, options = {}) {
                const fullUrl = url.startsWith("/api")
                    ? url
                    : `/api${url.startsWith("/") ? "" : "/"}${url}`;

                return await fetch(fullUrl, {
                    ...options,
                    credentials: "include",
                    headers: {
                        ...(options.headers || {}),
                        Accept: "application/json"
                    }
                });
            },

            unwrapDotNetList(data) {
    if (Array.isArray(data)) return data;
    if (data && Array.isArray(data.$values)) return data.$values;
    return [];
},

async loadPage() {
    const userId = this.getUserGuid();

    if (!userId) {
        this.errorMessage = "Please log in to access Continuing Education Credits.";
        return;
    }

    this.loading = true;
    this.errorMessage = "";

    try {
        const res = await this.apiFetch(`/api/PeerCertification/continuing-education/${userId}`);
        if (!res.ok) {
            this.errorMessage = await res.text();
            return;
        }

        const data = await res.json();

        this.certificationTracks = this.unwrapDotNetList(data.certificationTracks);
        this.ceDocs = this.unwrapDotNetList(data.documents);

        console.log("CE page response:", data);
        console.log("certificationTracks:", this.certificationTracks);
        console.log("documents:", this.ceDocs);
    } catch (e) {
        this.errorMessage = e?.message || "Failed to load continuing education page.";
    } finally {
        this.loading = false;
    }
},

            handleFileSelect(event) {
                this.selectedFile = event.target.files?.[0] || null;
            },

            async uploadDocument() {
                const userId = this.getUserGuid();
                if (!userId || !this.selectedFile) return;

                const formData = new FormData();
                formData.append("file", this.selectedFile);
                formData.append("docType", 9);
                formData.append("noOfCredits", this.credits || "");

                this.uploading = true;
                this.uploadMessage = "";

                try {
                    const res = await fetch(`/api/PeerCertification/continuing-education/upload/${userId}`, {
                        method: "POST",
                        credentials: "include",
                        body: formData
                    });

                    if (!res.ok) {
                        this.uploadMessage = await res.text();
                        return;
                    }

                    this.uploadMessage = "Document uploaded successfully.";
                    this.credits = "";
                    this.selectedFile = null;
                    if (this.$refs.fileInput) this.$refs.fileInput.value = "";
                    await this.loadPage();
                } catch (e) {
                    this.uploadMessage = e?.message || "Upload failed.";
                } finally {
                    this.uploading = false;
                }
            },

            downloadDoc(peerDocSysId) {
                window.open(`/api/PeerCertification/uploads/download/${peerDocSysId}`, "_blank");
            },

            async deleteDoc(peerDocSysId) {
                const userId = this.getUserGuid();
                if (!userId) return;
                if (!confirm("Remove this document?")) return;

                const res = await fetch(`/api/PeerCertification/uploads/${userId}/${peerDocSysId}`, {
                    method: "DELETE",
                    credentials: "include",
                    headers: { Accept: "application/json" }
                });

                if (res.ok) {
                    await this.loadPage();
                }
            },

            formatDate(value) {
                if (!value) return "—";
                const d = new Date(value);
                if (Number.isNaN(d.getTime())) return "—";
                return d.toLocaleDateString();
            },

            trackLabel(code) {
                const map = {
                    HIV: "HIV Peer Worker",
                    HCV: "HCV Peer Worker",
                    HR: "Harm Reduction Peer Worker",
                    PREP: "PrEP Peer Worker",
                    CJ: "Criminal Justice Peer Worker"
                };
                return map[code] || code;
            }
        }
    };</script>
<style scoped>
    .ce-page {
        padding: 24px;
        background: linear-gradient(180deg, #f6f8fb 0%, #f9fafc 100%);
        min-height: 100%;
    }

    .ce-card {
        max-width: 1180px;
        margin: 0 auto;
        background: linear-gradient(180deg, #fffefe 0%, #fcfcff 100%);
        border: 1px solid #e7eaf0;
        border-radius: 28px;
        padding: 30px;
        box-shadow: 0 18px 40px rgba(15, 23, 42, 0.06);
    }

    .ce-header {
        margin-bottom: 26px;
        padding-bottom: 8px;
    }

    .ce-badge {
        display: inline-flex;
        padding: 8px 14px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #43285d;
        font-size: 12px;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        margin-bottom: 14px;
    }

    .ce-header h1 {
        margin: 0 0 10px;
        font-size: 38px;
        font-weight: 900;
        color: #1f1630;
        line-height: 1.1;
    }

    .ce-subtitle {
        margin: 0;
        font-size: 16px;
        color: #667085;
        line-height: 1.75;
        max-width: 760px;
    }

    .section {
        margin-top: 24px;
    }

    .section-title-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        margin-bottom: 14px;
        flex-wrap: wrap;
    }

    .section h2 {
        margin: 0;
        font-size: 24px;
        font-weight: 900;
        color: #1f1630;
    }

    .section-chip {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-height: 32px;
        padding: 6px 12px;
        border-radius: 999px;
        background: #f5f3fb;
        color: #5b4b76;
        font-size: 12px;
        font-weight: 800;
        border: 1px solid #e6e1f0;
    }

    .track-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
        gap: 16px;
    }

    .track-card,
    .content-card,
    .upload-card,
    .state-card,
    .muted-box {
        background: #ffffff;
        border: 1px solid #e8ebf1;
        border-radius: 20px;
        padding: 20px;
        box-shadow: 0 6px 18px rgba(15, 23, 42, 0.03);
    }

    .track-card {
        background: linear-gradient(180deg, #ffffff 0%, #fafaff 100%);
    }

    .track-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 10px;
        margin-bottom: 10px;
    }

    .track-code {
        font-size: 20px;
        font-weight: 900;
        color: #43285d;
        letter-spacing: 0.02em;
    }

    .track-pill {
        padding: 6px 10px;
        border-radius: 999px;
        background: rgba(6, 95, 70, 0.10);
        color: #065f46;
        font-size: 11px;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0.04em;
    }

    .track-label {
        font-size: 14px;
        font-weight: 700;
        color: #374151;
        margin-bottom: 12px;
    }

    .track-date {
        display: flex;
        flex-direction: column;
        gap: 4px;
        font-size: 13px;
        color: #6b7280;
    }

        .track-date strong {
            font-size: 14px;
            color: #111827;
            font-weight: 800;
        }

    .content-card p,
    .content-card li {
        font-size: 15px;
        line-height: 1.9;
        color: #374151;
    }

    .ce-list {
        margin: 10px 0 0;
        padding-left: 20px;
    }

        .ce-list li {
            margin-bottom: 8px;
        }

    .important-note {
        margin-top: 18px;
        padding: 14px 16px;
        border-radius: 14px;
        background: #f8fafc;
        border: 1px solid #e7ebf2;
        font-size: 14px;
        line-height: 1.75;
        color: #374151;
        font-weight: 700;
    }

    .upload-card {
        background: linear-gradient(180deg, #ffffff 0%, #fcfcff 100%);
    }

    .upload-form {
        display: grid;
        grid-template-columns: 220px 1fr auto;
        gap: 16px;
        align-items: end;
    }

    .field {
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

        .field label {
            font-size: 14px;
            font-weight: 800;
            color: #344054;
        }

        .field input[type="number"] {
            height: 50px;
            border: 1px solid #d8dee8;
            border-radius: 14px;
            padding: 0 14px;
            font-size: 15px;
            background: #fff;
            color: #111827;
            outline: none;
            transition: border-color 0.18s ease, box-shadow 0.18s ease;
        }

            .field input[type="number"]:focus {
                border-color: #7c3aed;
                box-shadow: 0 0 0 4px rgba(124, 58, 237, 0.08);
            }

    .file-field {
        min-width: 0;
    }

    .hidden-file-input {
        display: none;
    }

    .file-picker-row {
        display: flex;
        align-items: center;
        gap: 12px;
        min-height: 50px;
    }

    .file-picker-btn {
        border: 1px solid #d7dce5;
        background: #ffffff;
        color: #1f2937;
        border-radius: 14px;
        height: 50px;
        padding: 0 18px;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
        transition: all 0.18s ease;
        white-space: nowrap;
    }

        .file-picker-btn:hover:not(:disabled) {
            border-color: #7c3aed;
            color: #43285d;
            background: #faf7ff;
        }

        .file-picker-btn:disabled {
            opacity: 0.65;
            cursor: not-allowed;
        }

    .file-name-box {
        flex: 1;
        min-width: 0;
        height: 50px;
        display: flex;
        align-items: center;
        padding: 0 14px;
        border: 1px solid #d8dee8;
        border-radius: 14px;
        background: #fbfcfe;
        color: #6b7280;
        font-size: 14px;
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
    }

    .actions {
        display: flex;
    }

    .upload-message {
        margin-top: 14px;
        font-size: 14px;
        font-weight: 700;
        padding: 12px 14px;
        border-radius: 14px;
    }

        .upload-message.success {
            background: rgba(6, 95, 70, 0.08);
            color: #065f46;
            border: 1px solid rgba(6, 95, 70, 0.15);
        }

        .upload-message.error {
            background: rgba(185, 28, 28, 0.08);
            color: #991b1b;
            border: 1px solid rgba(185, 28, 28, 0.14);
        }

    .table-wrap {
        overflow-x: auto;
        border: 1px solid #e6eaf0;
        border-radius: 18px;
        background: #fff;
    }

    .ce-table {
        width: 100%;
        border-collapse: collapse;
        min-width: 700px;
    }

        .ce-table th,
        .ce-table td {
            padding: 15px 16px;
            border-bottom: 1px solid #edf1f6;
            text-align: left;
            vertical-align: middle;
        }

        .ce-table th {
            background: #f8fafc;
            font-size: 13px;
            font-weight: 900;
            color: #374151;
            white-space: nowrap;
        }

        .ce-table tbody tr:nth-child(even) {
            background: #fcfcfd;
        }

        .ce-table tbody tr:hover {
            background: #f8f6fc;
        }

    .file-name-cell {
        font-weight: 700;
        color: #111827;
    }

    .row-actions {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
    }

    .btn {
        border: none;
        border-radius: 12px;
        padding: 10px 16px;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
        transition: transform 0.16s ease, box-shadow 0.16s ease, background 0.16s ease, border-color 0.16s ease;
    }

        .btn:hover:not(:disabled) {
            transform: translateY(-1px);
        }

        .btn:disabled {
            opacity: 0.65;
            cursor: not-allowed;
        }

    .btn-primary {
        background: #43285d;
        color: white;
        box-shadow: 0 10px 22px rgba(67, 40, 93, 0.22);
    }

        .btn-primary:hover:not(:disabled) {
            background: #51316f;
        }

    .btn-secondary {
        background: #fff;
        color: #111827;
        border: 1px solid #d1d5db;
    }

        .btn-secondary:hover:not(:disabled) {
            border-color: #7c3aed;
            color: #43285d;
            background: #faf7ff;
        }

    .btn-danger {
        background: #fff7f7;
        color: #991b1b;
        border: 1px solid #fecaca;
    }

        .btn-danger:hover:not(:disabled) {
            background: #fef2f2;
        }

    .btn-sm {
        padding: 8px 12px;
        font-size: 13px;
    }

    .state-card {
        color: #374151;
        font-weight: 700;
    }

    .error-card {
        background: #fff7f7;
        color: #991b1b;
        border-color: #fecaca;
    }

    .muted-box {
        color: #6b7280;
        font-weight: 600;
        background: #fbfcfe;
    }

    @media (max-width: 900px) {
        .upload-form {
            grid-template-columns: 1fr;
        }

        .file-picker-row {
            flex-direction: column;
            align-items: stretch;
        }

        .file-picker-btn,
        .file-name-box {
            width: 100%;
        }
    }

    @media (max-width: 640px) {
        .ce-page {
            padding: 14px;
        }

        .ce-card {
            padding: 20px 16px;
            border-radius: 20px;
        }

        .ce-header h1 {
            font-size: 30px;
        }

        .section h2 {
            font-size: 22px;
        }
    }
    .back-btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        border: 1px solid #d9deea;
        background: #ffffff;
        color: #344054;
        border-radius: 14px;
        padding: 10px 16px;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
        margin-bottom: 16px;
        transition: all 0.18s ease;
        box-shadow: 0 4px 12px rgba(15, 23, 42, 0.04);
    }

        .back-btn:hover {
            border-color: #7c3aed;
            color: #43285d;
            background: #faf7ff;
            transform: translateY(-1px);
        }

    .back-arrow {
        font-size: 16px;
        line-height: 1;
    }
</style>