<template>
    <div class="peer-detail-page">
        <div class="peer-detail-shell" v-if="!loading">
            <div class="detail-header-card">
                <div class="header-top">
                    <button class="back-btn" @click="$router.back()">← Back to Peer List</button>

                    <div class="header-actions">
                        <button class="save-btn" @click="saveChanges" :disabled="saving">
                            {{ saving ? "Saving..." : "Save Changes" }}
                        </button>
                    </div>
                </div>

                <div class="title-wrap">
                    <div class="page-badge">Peer Management</div>
                    <h1>{{ form.fullName || "Peer Detail" }}</h1>
                    <p class="page-subtitle">
                        Review and update peer certification application details.
                    </p>
                </div>

                <div class="review-top-grid">
                    <div class="summary-box readonly-box">
                        <span>Status</span>
                        <strong>{{ statusText }}</strong>
                    </div>

                    <div class="summary-box readonly-box">
                        <span>Last Login</span>
                        <strong>{{ formatDate(form.lastLoginDate) }}</strong>
                    </div>

                    <div class="summary-box readonly-box">
                        <span>Last Course Attended</span>
                        <strong>{{ formatDate(form.lastCourseAttendedDate) }}</strong>
                    </div>
                </div>

                <div class="review-divider"></div>

                <div class="section-inline-header">
                    <h2>Certification Review</h2>
                    <p>Update approval, application number, and certification dates.</p>
                </div>

                <div class="review-admin-grid four-col">
                    <div class="field-group">
                        <label>Application Status</label>

                        <select v-model="selectedAction"
                                @change="onActionChanged">

                            <option value="">
                                -- Select Status --
                            </option>

                            <option value="submitted">
                                Move to Submitted Applicants
                            </option>

                            <option value="approve">
                                Move to Successfully Approved
                            </option>

                            <option value="archive">
                                Move to Archived Applicants
                            </option>

                            <option value="disapprove">
                                Move to Disapproved Applicants
                            </option>

                            <option value="closed">
                                Move to Closed Applicants
                            </option>

                            <option value="lapsed">
                                Move to Lapsed Applicants
                            </option>
                        </select>
                    </div>

                    <div class="field-group">
                        <label>
                            Application Number
                            <span class="required-mark">*</span>
                        </label>
                        <input v-model="form.applicantNumber"
                               type="number"
                               min="1"
                               placeholder="Enter application number"
                               :class="{ 'input-error': applicantNumberError }" />

                        <span v-if="applicantNumberError" class="error-text">
                            Application Number is required.
                        </span>
                    </div>
                </div>

                <div class="cert-dates-section" v-if="hasAnyCertTrack">
                    <div class="section-subheading">Certification Dates</div>

                    <div class="form-grid three-col cert-dates-grid">
                        <div class="field-group" v-if="form.certHiv">
                            <label>HIV Cert Date</label>
                            <input v-model="form.certHivdate" type="date" />
                        </div>

                        <div class="field-group" v-if="form.certHcv">
                            <label>HCV Cert Date</label>
                            <input v-model="form.certHcvdate" type="date" />
                        </div>

                        <div class="field-group" v-if="form.certHr">
                            <label>HR Cert Date</label>
                            <input v-model="form.certHrdate" type="date" />
                        </div>

                        <div class="field-group" v-if="form.certPrep">
                            <label>PrEP Cert Date</label>
                            <input v-model="form.certPrepDate" type="date" />
                        </div>

                        <div class="field-group" v-if="form.certCriminalJustice">
                            <label>CJ Cert Date</label>
                            <input v-model="form.certCriminalJusticeDate" type="date" />
                        </div>
                    </div>
                </div>

                <div class="download-application-row">
                    <button class="download-pdf-btn" @click="downloadApplicationPdf" :disabled="loading">
                        Download Application PDF
                    </button>
                </div>

                <div class="field-group disapproval-box" v-if="form.disapprove === true">
                    <label>Disapproval Reason</label>
                    <textarea v-model="form.reasonDisapprv" rows="4" placeholder="Enter disapproval reason"></textarea>
                </div>
            </div>

            <div class="section-card">
                <h2>Applicant Information</h2>
                <div class="form-grid two-col">
                    <div class="field-group"><label>First Name</label><input :value="form.firstName || '—'" disabled /></div>
                    <div class="field-group"><label>Last Name</label><input :value="form.lastName || '—'" disabled /></div>
                    <div class="field-group"><label>Email</label><input :value="form.email || '—'" disabled /></div>
                    <div class="field-group"><label>Alt Email</label><input :value="form.altEmail || '—'" disabled /></div>
                    <div class="field-group"><label>Phone</label><input :value="form.phone || '—'" disabled /></div>
                    <div class="field-group"><label>Cell Phone</label><input :value="form.cellPhone || '—'" disabled /></div>
                    <div class="field-group"><label>Work Phone</label><input :value="form.workPhone || '—'" disabled /></div>
                    <div class="field-group"><label>Title</label><input :value="form.title || '—'" disabled /></div>
                    <div class="field-group full-width"><label>Address</label><input :value="form.address || '—'" disabled /></div>
                    <div class="field-group"><label>City</label><input :value="form.city || '—'" disabled /></div>
                    <div class="field-group"><label>State</label><input :value="form.state || '—'" disabled /></div>
                    <div class="field-group"><label>Zip</label><input :value="form.zip || '—'" disabled /></div>
                    <div class="field-group"><label>DOB</label><input :value="form.dob || '—'" disabled /></div>
                    <div class="field-group"><label>Agency Affiliation</label><input :value="form.agencyAffilation || '—'" disabled /></div>

                    <div class="field-group">
                        <label>Gender</label>
                        <input :value="getGenderText()" disabled />
                    </div>

                    <div class="field-group">
                        <label>Education</label>
                        <input :value="getEducationText()" disabled />
                    </div>

                    <div class="field-group">
                        <label>Ethnicity</label>
                        <input :value="getEthnicityText()" disabled />
                    </div>

                    <div class="field-group">
                        <label>Race</label>
                        <input :value="getRaceText()" disabled />
                    </div>

                    <div class="field-group full-width">
                        <label>Certification Track</label>
                        <input :value="getCertificationTrackText()" disabled />
                    </div>
                </div>
            </div>

            <div class="section-card">
                <h2>Lived Experience</h2>
                <div class="field-group">
                    <label>Commitment to Wellness</label>
                    <textarea v-model="form.experienceCommitment" rows="5"></textarea>
                </div>
                <div class="field-group">
                    <label>Challenges</label>
                    <textarea v-model="form.experienceChallenges" rows="5"></textarea>
                </div>
                <div class="field-group">
                    <label>Why Serve as Peer Worker</label>
                    <textarea v-model="form.experienceWhy" rows="5"></textarea>
                </div>
                <div class="field-group">
                    <label>Self Care</label>
                    <select v-model="form.selfCare">
                        <option :value="null">Select</option>
                        <option :value="true">Yes</option>
                        <option :value="false">No</option>
                    </select>
                </div>
            </div>

            <div class="section-card">
                <h2>Required Courses</h2>
                <div class="field-group inline-field">
                    <label>Required Courses Completed</label>
                    <select v-model="form.requiredCourses">
                        <option :value="true">Yes</option>
                        <option :value="false">No</option>
                    </select>
                </div>

                <div class="exam-grid" v-if="form.exams?.length">
                    <div class="exam-card" v-for="(exam, index) in form.exams" :key="index">
                        <div class="exam-label">Exam {{ index + 1 }}</div>
                        <div class="exam-value">{{ exam.status || "Not Started" }}</div>
                        <div class="exam-sub">{{ exam.completed ? "Completed" : "Not Completed" }}</div>
                    </div>
                </div>
            </div>

            <div class="section-card">
                <h2>Supervisor Information</h2>
                <div class="form-grid two-col">
                    <div class="field-group"><label>Supervisor First Name</label><input v-model="form.supvrFirstName" /></div>
                    <div class="field-group"><label>Supervisor Last Name</label><input v-model="form.supvrLastName" /></div>
                    <div class="field-group"><label>Supervisor Org</label><input v-model="form.supvrOrgName" /></div>
                    <div class="field-group"><label>Supervisor Phone</label><input v-model="form.supvrContPhone" /></div>
                    <div class="field-group"><label>Supervisor Email</label><input v-model="form.supvrContEmail" /></div>
                    <div class="field-group full-width"><label>Supervisor Address 1</label><input v-model="form.supvrContAddr1" /></div>
                    <div class="field-group full-width"><label>Supervisor Address 2</label><input v-model="form.supvrContAddr2" /></div>
                    <div class="field-group">
                        <label>Completed Practicum</label>
                        <select v-model="form.complPracticum">
                            <option :value="null">Select</option>
                            <option :value="true">Yes</option>
                            <option :value="false">No</option>
                        </select>
                    </div>
                    <div class="field-group">
                        <label>500 Hours Minimum</label>
                        <select v-model="form.complPracticumMin">
                            <option :value="null">Select</option>
                            <option :value="true">Yes</option>
                            <option :value="false">No</option>
                        </select>
                    </div>
                    <div class="field-group"><label>Practicum Begin Date</label><input v-model="form.practicumBdate" type="date" /></div>
                    <div class="field-group"><label>Practicum End Date</label><input v-model="form.practicumEdate" type="date" /></div>
                </div>
            </div>

            <div class="section-card">
                <h2>Documents</h2>
                <div v-if="form.uploads?.length" class="doc-list">
                    <div class="doc-row" v-for="doc in form.uploads" :key="doc.peerDocSysId">
                        <div>
                            <div class="doc-title">{{ doc.docTypeName }}</div>
                            <div class="doc-sub">{{ formatDate(doc.dateUpload) }}</div>
                        </div>

                        <div class="doc-actions">
                            <button class="doc-link-btn" @click="openDocument(doc)">
                                View
                            </button>

                            <a class="doc-link"
                               :href="getDownloadUrl(doc)"
                               target="_blank">
                                Download
                            </a>

                            <label class="doc-edit-btn">
                                Edit
                                <input type="file"
                                       hidden
                                       @change="handleReupload($event, doc)" />
                            </label>

                            <button class="doc-delete-btn"
                                    @click="deleteDocument(doc)">
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
                <div v-else class="empty-box">
                    No documents uploaded.
                </div>
            </div>

            <div class="section-card">
                <h2>Admin Notes</h2>
                <div class="field-group">
                    <label>Notes</label>
                    <textarea v-model="form.notes" rows="5"></textarea>
                </div>
            </div>

            <div class="submit-footer">
                <button class="final-submit-btn" @click="saveChanges" :disabled="saving">
                    {{ saving ? "Saving..." : "Submit Updates" }}
                </button>
            </div>
        </div>

        <div v-else class="loading-box">
            Loading peer details...
        </div>
    </div>
    <div v-if="showDocumentModal" class="doc-modal-overlay" @click.self="closeDocumentModal">
        <div class="doc-modal">
            <div class="doc-modal-header">
                <div>
                    <h3>{{ selectedDocument?.docTypeName || "Document Preview" }}</h3>
                    <p>{{ formatDate(selectedDocument?.dateUpload) }}</p>
                </div>

                <div class="doc-modal-actions">
                    <a v-if="selectedDocument"
                       :href="getDownloadUrl(selectedDocument)"
                       target="_blank"
                       class="doc-download-btn">
                        Download
                    </a>
                    <button class="doc-close-btn" @click="closeDocumentModal">×</button>
                </div>
            </div>

            <div class="doc-modal-body">
                <iframe v-if="selectedDocument && isPdfDocument(selectedDocument)"
                        :src="getPreviewUrl(selectedDocument)"
                        class="doc-frame">
                </iframe>

                <img v-else-if="selectedDocument && isImageDocument(selectedDocument)"
                     :src="getPreviewUrl(selectedDocument)"
                     class="doc-image"
                     alt="Document preview" />

                <div v-else class="doc-fallback">
                    <p>This file type cannot be previewed inside the page.</p>
                    <a v-if="selectedDocument"
                       :href="getDocumentUrl(selectedDocument)"
                       target="_blank"
                       download
                       class="doc-download-btn">
                        Download File
                    </a>
                </div>
            </div>
        </div>
    </div>
</template>
<script>export default {
    name: "ManagePeerDetail",
    props: ["userId"],
    data() {
    return {
        loading: true,
        saving: false,
        applicantNumberError: false,
        form: {},

        genderOptions: [],
        educationOptions: [],
        ethnicityOptions: [],
        raceOptions: [],
        selectedAction: "",

        showDocumentModal: false,
        selectedDocument: null
    };
},
    computed: {
        statusText() {
    if (this.form.closed === true) return "Closed";
    if (this.form.lapsed === true) return "Lapsed";
    if (this.form.approve === true) return "Approved";
    if (this.form.disapprove === true) return "Disapproved";

    if (
        this.form.isArchived === true ||
        (
            this.form.active === false &&
            this.form.closed !== true &&
            this.form.lapsed !== true
        )
    ) {
        return "Archived";
    }

    if (
        this.form.active === true &&
        Number(this.form.applicationPercentage || 0) === 100
    ) {
        return "Submitted";
    }

    return "In Progress";
},

        hasAnyCertTrack() {
            return !!(
                this.form.certHiv ||
                this.form.certHcv ||
                this.form.certHr ||
                this.form.certPrep ||
                this.form.certCriminalJustice
            );
        }
    },
    async mounted() {
    await this.fetchLookups();
    await this.fetchDetail();
},
    methods: {
        unwrapList(data) {
            if (Array.isArray(data)) return data;
            if (data && Array.isArray(data.$values)) return data.$values;
            return [];
        },async handleReupload(event, doc) {
    const file = event.target.files[0];
    if (!file) return;

    try {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("docType", doc.peerDocId);

        const res = await fetch(`/api/PeerCertification/uploads/${this.userId}`, {
            method: "POST",
            credentials: "include",
            body: formData
        });

        if (!res.ok) {
            const msg = await res.text();
            throw new Error(msg || "Upload failed");
        }

        alert("Document updated successfully ✅");

        await this.fetchDetail(); // refresh list
    } catch (err) {
        console.error(err);
        alert(err.message || "Re-upload failed");
    }
},
downloadApplicationPdf() {
    window.open(`/api/PeerCertification/admin/manage-peer-detail/${this.userId}/download-pdf`, "_blank");
},
async deleteDocument(doc) {
    if (!confirm("Are you sure you want to delete this document?")) return;

    try {
        const res = await fetch(
            `/api/PeerCertification/uploads/${this.userId}/${doc.peerDocSysId}`,
            {
                method: "DELETE",
                credentials: "include"
            }
        );

        if (!res.ok) {
            const msg = await res.text();
            throw new Error(msg || "Delete failed");
        }

        alert("Document deleted successfully 🗑");

        await this.fetchDetail();
    } catch (err) {
        console.error(err);
        alert(err.message || "Delete failed");
    }
},
    openDocument(doc) {
    this.selectedDocument = doc;
    this.showDocumentModal = true;
},

closeDocumentModal() {
    this.showDocumentModal = false;
    this.selectedDocument = null;
},

getPreviewUrl(doc) {
    if (!doc) return "";
    return `/api/PeerCertification/uploads/preview/${doc.peerDocSysId}`;
},

getDownloadUrl(doc) {
    if (!doc) return "";
    return `/api/PeerCertification/uploads/download/${doc.peerDocSysId}`;
},

getFileExtension(doc) {
    const path = doc?.docPath || "";
    const fileName = path.split("/").pop() || "";
    const parts = fileName.split(".");
    return parts.length > 1 ? parts.pop().toLowerCase() : "";
},

isPdfDocument(doc) {
    return this.getFileExtension(doc) === "pdf";
},

isImageDocument(doc) {
    const ext = this.getFileExtension(doc);
    return ["png", "jpg", "jpeg", "webp", "gif"].includes(ext);
},

canPreviewDocument(doc) {
    return this.isPdfDocument(doc) || this.isImageDocument(doc);
},
    async fetchLookups() {
    try {
        const [gendersRes, educationsRes, ethnicitiesRes, racesRes] = await Promise.all([
            fetch("/api/Lookup/genders", {
                credentials: "include",
                headers: { Accept: "application/json" }
            }),
            fetch("/api/Lookup/educations", {
                credentials: "include",
                headers: { Accept: "application/json" }
            }),
            fetch("/api/Lookup/ethnicities", {
                credentials: "include",
                headers: { Accept: "application/json" }
            }),
            fetch("/api/Lookup/races", {
                credentials: "include",
                headers: { Accept: "application/json" }
            })
        ]);

        const gendersData = gendersRes.ok ? await gendersRes.json() : [];
        const educationsData = educationsRes.ok ? await educationsRes.json() : [];
        const ethnicitiesData = ethnicitiesRes.ok ? await ethnicitiesRes.json() : [];
        const racesData = racesRes.ok ? await racesRes.json() : [];

        this.genderOptions = this.unwrapList(gendersData);
        this.educationOptions = this.unwrapList(educationsData);
        this.ethnicityOptions = this.unwrapList(ethnicitiesData);
        this.raceOptions = this.unwrapList(racesData);

        console.log("genderOptions", this.genderOptions);
        console.log("educationOptions", this.educationOptions);
        console.log("ethnicityOptions", this.ethnicityOptions);
        console.log("raceOptions", this.raceOptions);
    } catch (error) {
        console.error("fetchLookups error:", error);
        this.genderOptions = [];
        this.educationOptions = [];
        this.ethnicityOptions = [];
        this.raceOptions = [];
    }
},

getLookupValue(options, code) {
    const list = this.unwrapList(options);

    if (code === null || code === undefined || code === "") return "—";
    if (!Array.isArray(list)) return code;

    const match = list.find(x =>
        String(x.code ?? x.Code) === String(code)
    );

    return match ? (match.value ?? match.Value) : code;
},

getGenderText() {
    return this.getLookupValue(this.genderOptions, this.form.gender);
},

getEducationText() {
    return this.getLookupValue(this.educationOptions, this.form.education);
},

getEthnicityText() {
    return this.getLookupValue(this.ethnicityOptions, this.form.ethnicity);
},

getRaceText() {
    return this.getLookupValue(this.raceOptions, this.form.race);
},
getCertificationTrackText() {
    if (!this.form.certificationTracks || !this.form.certificationTracks.length) {
        return "—";
    }

    return this.form.certificationTracks.map(t => t.code).join(", ");
},
onActionChanged() {
    // Reset all mutually exclusive statuses first.
    this.form.approve = null;
    this.form.disapprove = null;
    this.form.closed = null;
    this.form.lapsed = null;
    this.form.isArchived = false;
    this.form.reasonDisapprv = null;

    switch (this.selectedAction) {
        case "submitted":
            this.form.active = true;
            this.form.applicationPercentage = 100;
            break;

        case "approve":
            this.form.approve = true;
            this.form.active = true;
            break;

        case "disapprove":
            this.form.disapprove = true;
            this.form.active = true;
            break;

        case "archive":
            this.form.isArchived = true;
            this.form.active = false;
            break;

        case "closed":
            this.form.closed = true;

            // Neither true nor false because this is not active/submitted
            // and is not archived.
            this.form.active = null;
            break;

        case "lapsed":
            this.form.lapsed = true;

            // Neither true nor false because this is not active/submitted
            // and is not archived.
            this.form.active = null;
            break;

        default:
            break;
    }
},
setApprove(value) {
            if (value === true) {
                this.form.approve = true;
                this.form.disapprove = null;
                this.form.isArchived = false;
                this.form.active = true;
            } else {
                this.form.approve = null;
            }
        },

        setDisapprove(value) {
            if (value === true) {
                this.form.disapprove = true;
                this.form.approve = null;
                this.form.isArchived = false;
                this.form.active = true;
            } else {
                this.form.disapprove = null;
            }
        },

        setArchive(value) {
            if (value === true) {
                this.form.isArchived = true;
                this.form.active = false;
                this.form.approve = null;
                this.form.disapprove = null;
            } else {
                this.form.isArchived = false;

                if (this.form.active === false) {
                    this.form.active = true;
                }
            }
        },

        async fetchDetail() {
            this.loading = true;

            try {
                const response = await fetch(`/api/PeerCertification/admin/manage-peer-detail/${this.userId}`, {
                    credentials: "include",
                    headers: {
                        Accept: "application/json"
                    }
                });

                if (!response.ok) {
                    const msg = await response.text();
                    throw new Error(msg || "Failed to load peer detail.");
                }

                const data = await response.json();

                const certificationTracks = this.unwrapList(data.certificationTracks);
                const uploads = this.unwrapList(data.uploads);
                const exams = this.unwrapList(data.exams);

                this.form = {
                    ...data,
                    certificationTracks,
                    uploads,
                    exams,

                    dob: this.toInputDate(data.dob),
                    certHivdate: this.toInputDate(data.certHivdate),
                    certHcvdate: this.toInputDate(data.certHcvdate),
                    certHrdate: this.toInputDate(data.certHrdate),
                    certPrepDate: this.toInputDate(data.certPrepDate),
                    certCriminalJusticeDate: this.toInputDate(data.certCriminalJusticeDate),
                    practicumBdate: this.toInputDate(data.practicumBdate),
                    practicumEdate: this.toInputDate(data.practicumEdate),

                    certHiv: certificationTracks.some(x => x.code === "HIV"),
                    certHcv: certificationTracks.some(x => x.code === "HCV"),
                    certHr: certificationTracks.some(x => x.code === "HR"),
                    certPrep: certificationTracks.some(x => x.code === "PREP"),
                    certCriminalJustice: certificationTracks.some(x => x.code === "CJ"),

                    isArchived:
    data.active === false &&
    data.approve !== true &&
    data.disapprove !== true &&
    data.closed !== true &&
    data.lapsed !== true
};

if (data.closed === true) {
    this.selectedAction = "closed";
} else if (data.lapsed === true) {
    this.selectedAction = "lapsed";
} else if (data.approve === true) {
    this.selectedAction = "approve";
} else if (data.disapprove === true) {
    this.selectedAction = "disapprove";
} else if (
    data.active === false &&
    data.approve !== true &&
    data.disapprove !== true &&
    data.closed !== true &&
    data.lapsed !== true
) {
    this.selectedAction = "archive";
} else if (
    data.active === true &&
    Number(data.applicationPercentage || 0) === 100
) {
    this.selectedAction = "submitted";
} else {
    this.selectedAction = "";
}

this.applicantNumberError = false;
            } catch (error) {
                console.error("fetchDetail error:", error);
                alert(error?.message || "Failed to load peer detail.");
            } finally {
                this.loading = false;
            }
        },

        async saveChanges() {
            if (!this.selectedAction) {
    alert("Please select an application status.");
    return;
}   
            this.applicantNumberError = !this.form.applicantNumber;

            if (this.applicantNumberError) {
                alert("Application Number is required.");
                return;
            }

            this.saving = true;

            try {
               const payload = {
    ApplicantNumber: this.form.applicantNumber,

    Approve:
        this.selectedAction === "approve"
            ? true
            : null,

    Disapprove:
        this.selectedAction === "disapprove"
            ? true
            : null,

    Closed:
        this.selectedAction === "closed"
            ? true
            : null,

    Lapsed:
        this.selectedAction === "lapsed"
            ? true
            : null,

    Active:
        this.selectedAction === "archive"
            ? false
            : (
                this.selectedAction === "closed" ||
                this.selectedAction === "lapsed"
            )
                ? null
                : true,

    ApplicationPercentage:
        this.selectedAction === "submitted"
            ? 100
            : this.form.applicationPercentage,

    ReasonDisapprv:
        this.selectedAction === "disapprove"
            ? this.form.reasonDisapprv
            : null,

    Notes: this.form.notes,

    CertHivdate: this.form.certHivdate || null,
    CertHcvdate: this.form.certHcvdate || null,
    CertHrdate: this.form.certHrdate || null,
    CertPrepDate: this.form.certPrepDate || null,
    CertCriminalJusticeDate: this.form.certCriminalJusticeDate || null,

    ExperienceCommitment: this.form.experienceCommitment || null,
    ExperienceChallenges: this.form.experienceChallenges || null,
    ExperienceWhy: this.form.experienceWhy || null,
    SelfCare: this.form.selfCare,

    RequiredCourses: this.form.requiredCourses,

    SupvrOrgName: this.form.supvrOrgName || null,
    SupvrFirstName: this.form.supvrFirstName || null,
    SupvrLastName: this.form.supvrLastName || null,
    SupvrContAddr1: this.form.supvrContAddr1 || null,
    SupvrContAddr2: this.form.supvrContAddr2 || null,
    SupvrContPhone: this.form.supvrContPhone || null,
    SupvrContEmail: this.form.supvrContEmail || null,

    ComplPracticum: this.form.complPracticum,
    ComplPracticumMin: this.form.complPracticumMin,
    PracticumBDate: this.form.practicumBdate || null,
    PracticumEDate: this.form.practicumEdate || null
};

                const response = await fetch(`/api/PeerCertification/admin/manage-peer-detail/${this.userId}`, {
                    method: "PUT",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                        Accept: "application/json"
                    },
                    body: JSON.stringify(payload)
                });

                if (!response.ok) {
                    const msg = await response.text();
                    throw new Error(msg || "Update failed.");
                }

                alert("Peer details updated successfully.");
                await this.fetchDetail();
            } catch (error) {
                console.error("saveChanges error:", error);
                alert(error?.message || "Unable to save changes.");
            } finally {
                this.saving = false;
            }
        },

        toInputDate(value) {
            if (!value) return "";
            const d = new Date(value);
            if (Number.isNaN(d.getTime())) return "";
            return d.toISOString().split("T")[0];
        },

        formatDate(value) {
            if (!value) return "—";
            const d = new Date(value);
            if (Number.isNaN(d.getTime())) return "—";
            return d.toLocaleDateString();
        }
    }
};</script>

<style scoped>
    .peer-detail-page {
        padding: 24px;
        background: #f6f8fb;
        min-height: 100%;
    }

    .peer-detail-shell {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }

    .detail-header-card,
    .section-card {
        background: #fff;
        border: 1px solid #e5e7eb;
        border-radius: 22px;
        padding: 24px;
        box-shadow: 0 10px 28px rgba(15, 23, 42, 0.05);
    }

    .header-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 20px;
        gap: 12px;
    }

    .back-btn,
    .save-btn,
    .final-submit-btn {
        border: none;
        background: #4f2d6f;
        color: #fff;
        border-radius: 14px;
        padding: 12px 20px;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
        min-width: 170px;
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
        margin-bottom: 14px;
    }

    .title-wrap h1 {
        margin: 0 0 8px;
        font-size: 34px;
        font-weight: 900;
        color: #1f1630;
    }

    .page-subtitle {
        margin: 0;
        color: #667085;
        font-size: 15px;
        line-height: 1.7;
    }

    .review-top-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 14px;
        margin-top: 24px;
    }

    .summary-box {
        min-width: 180px;
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        padding: 14px 18px;
    }

        .summary-box span {
            display: block;
            font-size: 12px;
            font-weight: 700;
            color: #6b7280;
            text-transform: uppercase;
            margin-bottom: 6px;
        }

        .summary-box strong {
            font-size: 18px;
            color: #111827;
        }

    .readonly-box {
        background: #f8fafc;
    }

    .review-divider {
        height: 1px;
        background: #eceff3;
        margin: 26px 0 22px;
    }

    .section-inline-header {
        margin-bottom: 18px;
    }

        .section-inline-header h2 {
            margin: 0 0 6px;
            font-size: 24px;
            font-weight: 900;
            color: #1f1630;
        }

        .section-inline-header p {
            margin: 0;
            font-size: 14px;
            color: #6b7280;
        }

    .review-admin-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 18px;
        align-items: start;
    }

    .form-grid {
        display: grid;
        gap: 16px;
    }

    .two-col {
        grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .three-col {
        grid-template-columns: repeat(3, minmax(0, 1fr));
    }

    .field-group {
        display: flex;
        flex-direction: column;
        gap: 8px;
        margin-bottom: 14px;
    }

        .field-group label {
            font-size: 13px;
            font-weight: 800;
            color: #374151;
        }

        .field-group input,
        .field-group select,
        .field-group textarea {
            border: 1px solid #d1d5db;
            border-radius: 14px;
            padding: 12px 14px;
            font-size: 14px;
            color: #111827;
            background: #fff;
            outline: none;
        }

        .field-group textarea {
            resize: vertical;
        }

    .full-width {
        grid-column: 1 / -1;
    }

    .toggle-row {
        display: flex;
        gap: 10px;
    }

    .toggle-btn {
        min-width: 86px;
        height: 44px;
        border-radius: 12px;
        border: 1px solid #d1d5db;
        background: #fff;
        color: #374151;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
        transition: all 0.2s ease;
    }

        .toggle-btn:hover {
            border-color: #4f2d6f;
            color: #4f2d6f;
        }

        .toggle-btn.active {
            background: #4f2d6f;
            border-color: #4f2d6f;
            color: #fff;
            box-shadow: 0 8px 18px rgba(79, 45, 111, 0.18);
        }

        .toggle-btn.danger.active {
            background: #b42318;
            border-color: #b42318;
        }

    .required-mark {
        color: #dc2626;
        margin-left: 4px;
        font-weight: 900;
    }

    .input-error {
        border-color: #dc2626 !important;
        background: #fff8f8 !important;
    }

    .error-text {
        font-size: 12px;
        color: #dc2626;
        font-weight: 700;
    }

    .cert-dates-section {
        margin-top: 12px;
    }

    .section-subheading {
        margin-bottom: 12px;
        font-size: 13px;
        font-weight: 900;
        color: #6b7280;
        text-transform: uppercase;
        letter-spacing: 0.04em;
    }

    .disapproval-box {
        margin-top: 8px;
    }

    .doc-list {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

    .doc-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        border: 1px solid #e5e7eb;
        border-radius: 16px;
        padding: 14px 16px;
        background: #fafafa;
    }

    .doc-title {
        font-size: 14px;
        font-weight: 800;
        color: #111827;
    }

    .doc-sub {
        font-size: 12px;
        color: #6b7280;
        margin-top: 4px;
    }

    .doc-link {
        color: #43285d;
        font-weight: 800;
        text-decoration: none;
    }

    .exam-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: 14px;
    }

    .exam-card {
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        padding: 16px;
        background: #fafafa;
    }

    .exam-label {
        font-size: 12px;
        font-weight: 800;
        color: #6b7280;
        text-transform: uppercase;
        margin-bottom: 6px;
    }

    .exam-value {
        font-size: 16px;
        font-weight: 900;
        color: #111827;
    }

    .exam-sub {
        margin-top: 4px;
        font-size: 13px;
        color: #6b7280;
    }

    .submit-footer {
        display: flex;
        justify-content: flex-end;
    }

    .loading-box,
    .empty-box {
        background: #fff;
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        padding: 24px;
        color: #374151;
        font-weight: 700;
    }

    @media (max-width: 1100px) {
        .review-top-grid,
        .review-admin-grid,
        .three-col,
        .two-col,
        .four-col {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 900px) {
        .header-top,
        .submit-footer {
            flex-direction: column;
        }

        .back-btn,
        .save-btn,
        .final-submit-btn {
            width: 100%;
        }
    }
    .four-col {
        grid-template-columns: repeat(4, minmax(0, 1fr));
    }
    .toggle-btn.warning.active {
        background: #9a6700;
        border-color: #9a6700;
    }
    .doc-actions {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .doc-link-btn {
        border: none;
        background: #4f2d6f;
        color: #fff;
        border-radius: 10px;
        padding: 8px 14px;
        font-size: 13px;
        font-weight: 800;
        cursor: pointer;
    }

    .doc-modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.55);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999;
        padding: 24px;
    }

    .doc-modal {
        width: min(1100px, 96vw);
        height: min(90vh, 900px);
        background: #fff;
        border-radius: 22px;
        overflow: hidden;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.22);
        display: flex;
        flex-direction: column;
    }

    .doc-modal-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        padding: 18px 22px;
        border-bottom: 1px solid #e5e7eb;
        background: #fafafa;
    }

        .doc-modal-header h3 {
            margin: 0 0 4px;
            font-size: 18px;
            font-weight: 900;
            color: #1f1630;
        }

        .doc-modal-header p {
            margin: 0;
            font-size: 13px;
            color: #6b7280;
        }

    .doc-modal-actions {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .doc-download-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        text-decoration: none;
        border: none;
        background: #4f2d6f;
        color: #fff;
        border-radius: 12px;
        padding: 10px 16px;
        font-size: 13px;
        font-weight: 800;
        cursor: pointer;
    }

    .doc-close-btn {
        width: 42px;
        height: 42px;
        border: none;
        border-radius: 12px;
        background: #f3f4f6;
        color: #111827;
        font-size: 22px;
        cursor: pointer;
        font-weight: 700;
    }

    .doc-modal-body {
        flex: 1;
        background: #f8fafc;
        display: flex;
        align-items: center;
        justify-content: center;
        overflow: hidden;
    }

    .doc-frame {
        width: 100%;
        height: 100%;
        border: none;
        background: #fff;
    }

    .doc-image {
        max-width: 100%;
        max-height: 100%;
        object-fit: contain;
        background: #fff;
    }

    .doc-fallback {
        text-align: center;
        padding: 30px;
    }

        .doc-fallback p {
            margin-bottom: 16px;
            color: #4b5563;
            font-size: 14px;
            font-weight: 700;
        }
    .doc-edit-btn {
        background: #2563eb;
        color: #fff;
        border-radius: 10px;
        padding: 8px 14px;
        font-size: 13px;
        font-weight: 800;
        cursor: pointer;
    }

    .doc-delete-btn {
        background: #b42318;
        color: #fff;
        border: none;
        border-radius: 10px;
        padding: 8px 14px;
        font-size: 13px;
        font-weight: 800;
        cursor: pointer;
    }
    .download-application-row {
        margin-top: 18px;
        display: flex;
        justify-content: flex-start;
    }

    .download-pdf-btn {
        border: none;
        background: #1d4ed8;
        color: #fff;
        border-radius: 12px;
        padding: 12px 18px;
        font-size: 14px;
        font-weight: 800;
        cursor: pointer;
        box-shadow: 0 10px 22px rgba(29, 78, 216, 0.18);
        transition: all 0.18s ease;
    }

        .download-pdf-btn:hover {
            background: #1e40af;
            transform: translateY(-1px);
        }
        .download-pdf-btn:disabled {
            opacity: 0.7;
            cursor: not-allowed;
            transform: none;
        }
</style>