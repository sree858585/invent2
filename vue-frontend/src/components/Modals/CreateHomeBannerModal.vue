<template>
    <div class="modal-overlay">
        <div class="modal fade-in">
            <!-- Close -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- Header -->
            <header class="modal-header">
                <div>
                    <h2>Create Home Banner</h2>
                    <p class="modal-subtitle">
                        Add a banner for the home page carousel and link it to either a course or an info modal.
                    </p>
                </div>
                <span class="modal-badge">Admin • Banner Manager</span>
            </header>

            <form @submit.prevent="submitBanner">
                <!-- BASIC INFORMATION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Basic Information</h3>
                        <p>Enter banner details and select what should happen when the user clicks it.</p>
                    </div>

                    <div class="form-group">
                        <label>Banner Name <span class="required">*</span></label>
                        <input v-model.trim="form.bannerName" placeholder="Enter banner name" required />
                    </div>

                    <div class="form-group">
                        <label>Action Type <span class="required">*</span></label>
                        <select v-model="form.actionType" required>
                            <option value="">Select</option>
                            <option value="Info">Info Modal</option>
                            <option value="Course">Open Course</option>
                        </select>
                    </div>

                    <div class="form-group" v-if="form.actionType === 'Course'">
                        <label>Tag Course <span class="required">*</span></label>
                        <select v-model="form.courseSysId">
                            <option value="">Select Course</option>
                            <option v-for="c in courses"
                                    :key="c.courseSysId"
                                    :value="c.courseSysId">
                                {{ c.subjectTitle || "Untitled Course" }} | {{ formatDate(c.courseDate) }}
                            </option>
                        </select>
                    </div>

                    <div class="form-group" v-if="form.actionType === 'Info'">
                        <label>Modal Title <span class="required">*</span></label>
                        <input v-model.trim="form.modalTitle" placeholder="Enter modal title" />
                    </div>

                    <div class="form-group" v-if="form.actionType === 'Info'">
                        <label>Modal Content <span class="required">*</span></label>

                        <quill-editor v-model:content="form.modalBodyHtml"
                                      contentType="html"
                                      theme="snow"
                                      class="quill-box resizable" />
                    </div>

                    <div class="form-row">
                        <div class="form-group">
                            <label>Display Order</label>
                            <input type="number" v-model="form.displayOrder" min="1" />
                        </div>

                        <div class="form-group active-group">
                            <label>Status</label>
                            <label class="checkbox-pill">
                                <input type="checkbox" v-model="form.active" />
                                <span>Active</span>
                            </label>
                        </div>

                        <div class="form-row">
                            <div class="form-group">
                                <label>Start Date</label>
                                <input type="date" v-model="form.startDate" />
                            </div>

                            <div class="form-group">
                                <label>End Date</label>
                                <input type="date" v-model="form.endDate" />
                            </div>
                        </div>
                    </div>
                </section>

                <!-- IMAGE -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Banner Image</h3>
                        <p>Upload a JPG or PNG image with exact size 1600 × 900 and under 500 KB.</p>
                    </div>

                    <div class="form-group">
                        <label>Banner Image <span class="required">*</span></label>

                        <div class="image-uploader">
                            <input ref="fileInput"
                                   class="file-hidden"
                                   type="file"
                                   accept=".png,.jpg,.jpeg,image/png,image/jpeg"
                                   @change="onImageSelected" />

                            <div class="image-preview" v-if="imagePreviewUrl">
                                <img :src="imagePreviewUrl" alt="Banner preview" />
                                <button type="button"
                                        class="img-remove"
                                        @click="clearImage"
                                        title="Remove image">
                                    ✕
                                </button>
                            </div>


                            <div class="image-empty" v-else>
                                <div class="img-icon">🖼️</div>
                                <div class="img-text">
                                    <div class="img-title">Upload a home banner</div>
                                    <div class="img-sub">JPG/PNG • Max 500 KB • Required 1600×900</div>
                                </div>
                            </div>

                            <div class="img-actions">
                                <button type="button" class="btn-upload" @click="triggerFilePicker">
                                    Choose Image
                                </button>

                                <button type="button"
                                        class="btn-upload subtle"
                                        v-if="imagePreviewUrl"
                                        @click="triggerFilePicker">
                                    Change
                                </button>
                            </div>
                        </div>

                        <small v-if="imageError" class="error-text">{{ imageError }}</small>
                    </div>
                </section>

                <!-- BUTTONS -->
                <div class="button-group">
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                    <button type="submit" class="btn-primary">Create Banner</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";
    import { QuillEditor } from "@vueup/vue-quill";

    export default {
    name: "CreateHomeBannerModal",
    components: { QuillEditor },
    emits: ["close", "created"],

        data() {
            return {
                courses: [],
                imageFile: null,
                imagePreviewUrl: "",
                imageError: "",
                form: {
                    bannerName: "",
                    actionType: "",
                    courseSysId: "",
                    modalTitle: "",
                    modalBodyHtml: "",
                    displayOrder: 1,
                    active: true,
                    startDate: "",
                    endDate: ""
                }
            };
        },

        async mounted() {
            try {
                const res = await apiClient.get("/Course/all", {
                    params: {
                        page: 1,
                        pageSize: 500
                    }
                });

                this.courses = res.data?.data?.$values ?? res.data?.data ?? [];
            } catch (err) {
                console.error("Failed to load courses:", err);
                this.courses = [];
            }
        },

        beforeUnmount() {
            if (this.imagePreviewUrl?.startsWith("blob:")) {
                URL.revokeObjectURL(this.imagePreviewUrl);
            }
        },

        methods: {
            formatDate(d) {
                if (!d) return "";
                return new Date(d).toLocaleDateString();
            },

            triggerFilePicker() {
                if (this.$refs.fileInput) {
                    this.$refs.fileInput.value = null;
                }
                this.$refs.fileInput?.click();
            },

            onImageSelected(e) {
                this.imageError = "";

                const file = e.target.files && e.target.files.length ? e.target.files[0] : null;

                if (!file) {
                    this.imageFile = null;
                    this.imagePreviewUrl = "";
                    return;
                }

                console.log("Selected file:", file);
                console.log("File type:", file.type);
                console.log("File size:", file.size);

                const allowedTypes = ["image/png", "image/jpeg", "image/jpg"];
                if (!allowedTypes.includes(file.type)) {
                    this.imageError = "Only JPG and PNG images are allowed.";
                    this.imageFile = null;
                    this.clearImage();
                    return;
                }

                const maxBytes = 500 * 1024;
                if (file.size > maxBytes) {
                    this.imageError = "Image size must be under 500 KB.";
                    this.imageFile = null;

                    if (this.imagePreviewUrl?.startsWith("blob:")) {
                        URL.revokeObjectURL(this.imagePreviewUrl);
                    }
                    this.imagePreviewUrl = "";

                    if (this.$refs.fileInput) {
                        this.$refs.fileInput.value = "";
                    }
                    return;
                }

                this.imageFile = file;

                if (this.imagePreviewUrl?.startsWith("blob:")) {
                    URL.revokeObjectURL(this.imagePreviewUrl);
                }

                this.imagePreviewUrl = URL.createObjectURL(file);

                console.log("imageFile set:", this.imageFile);
            },

            clearImage() {
                this.imageFile = null;

                if (this.imagePreviewUrl?.startsWith("blob:")) {
                    URL.revokeObjectURL(this.imagePreviewUrl);
                }

                this.imagePreviewUrl = "";

                if (this.$refs.fileInput) {
                    this.$refs.fileInput.value = "";
                }
            },

            async submitBanner() {
                try {
                    if (!this.form.bannerName.trim()) {
                        alert("Banner name is required.");
                        return;
                    }

                    if (!this.form.actionType) {
                        alert("Please select an action type.");
                        return;
                    }

                    if (this.form.actionType === "Course" && !this.form.courseSysId) {
                        alert("Please select a course.");
                        return;
                    }

                    if (this.form.actionType === "Info" && !this.form.modalTitle.trim()) {
                        alert("Modal title is required.");
                        return;
                    }

                    const plainContent = (this.form.modalBodyHtml || "")
    .replace(/<(.|\n)*?>/g, "")
    .replace(/&nbsp;/g, " ")
    .trim();

if (this.form.actionType === "Info" && !plainContent) {
    alert("Modal content is required.");
    return;
}

                    if (!this.imageFile) {
                        alert(this.imageError || "Please upload a valid banner image.");
                        return;
                    }

                    const fd = new FormData();
                    fd.append("BannerName", this.form.bannerName);
                    fd.append("ActionType", this.form.actionType);
                    fd.append("DisplayOrder", this.form.displayOrder);
                    fd.append("Active", this.form.active);
                    fd.append("File", this.imageFile);

                    if (this.form.courseSysId) {
                        fd.append("CourseSysId", this.form.courseSysId);
                    }

                    if (this.form.modalTitle) {
                        fd.append("ModalTitle", this.form.modalTitle);
                    }

                    if (this.form.modalBodyHtml) {
                        fd.append("ModalBodyHtml", this.form.modalBodyHtml);
                    }

                    if (this.form.startDate) {
                        fd.append("StartDate", this.form.startDate);
                    }

                    if (this.form.endDate) {
                        fd.append("EndDate", this.form.endDate);
                    }

                    await apiClient.post("/HomeBanner/create", fd, {
                        headers: {
                            "Content-Type": "multipart/form-data"
                        }
                    });

                    alert("Banner created successfully");
                    this.$emit("created");
                    this.$emit("close");
                } catch (err) {
                    console.error("Failed to create banner:", err);
                    alert(err?.response?.data?.message || "Failed to create banner");
                }
            }
        }
    };</script>

<style scoped>
    /* OVERLAY */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: radial-gradient(circle at top, rgba(15,23,42,0.3), rgba(15,23,42,0.75));
        backdrop-filter: blur(6px);
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 24px;
        z-index: 9999;
    }

    /* MODAL */
    .modal {
        position: relative;
        background: #ffffff;
        border-radius: 24px;
        width: 1000px;
        max-width: 100%;
        max-height: 90vh;
        overflow-y: auto;
        padding: 32px;
        box-shadow: 0 24px 60px rgba(15,23,42,0.25), 0 0 0 1px rgba(148,163,184,0.35);
    }

    /* CLOSE BUTTON */
    .close-btn {
        position: absolute;
        top: 14px;
        right: 16px;
        background: rgba(255,255,255,0.9);
        border: none;
        border-radius: 50%;
        width: 34px;
        height: 34px;
        font-size: 20px;
        cursor: pointer;
        z-index: 5;
    }

        .close-btn:hover {
            background: #ffebee;
            color: #c62828;
        }

    /* HEADER */
    .modal-header {
        background: #43285D;
        color: white;
        margin: -32px -32px 24px -32px;
        padding: 28px 40px;
        border-top-left-radius: 24px;
        border-top-right-radius: 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .modal-header h2 {
            font-size: 28px;
            font-weight: 700;
            margin: 0;
        }

    .modal-subtitle {
        font-size: 14px;
        opacity: 0.9;
        margin-top: 6px;
    }

    .modal-badge {
        background: rgba(255,255,255,0.15);
        padding: 8px 18px;
        border-radius: 999px;
        font-weight: 600;
        font-size: 12px;
        color: white;
    }

    /* SECTION CARD */
    .section-card {
        background: white;
        border-radius: 18px;
        padding: 20px;
        border: 1px solid #e5e7eb;
        margin-bottom: 20px;
        box-shadow: 0 12px 30px rgba(15,23,42,0.08);
    }

    .section-header h3 {
        font-size: 17px;
        font-weight: 600;
        margin: 0;
    }

    .section-header p {
        font-size: 13px;
        color: #6b7280;
        margin: 4px 0 12px;
    }

    /* FORM */
    .form-group {
        margin-bottom: 16px;
    }

    .form-row {
        display: grid;
        grid-template-columns: 1fr 220px;
        gap: 16px;
    }

    input,
    select,
    textarea {
        width: 100%;
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
        font-size: 14px;
        box-sizing: border-box;
    }

        textarea.resizable-textarea {
            min-height: 130px;
            resize: vertical;
        }

    label {
        display: block;
        margin-bottom: 8px;
        font-weight: 600;
        color: #374151;
    }

    .required {
        color: #ef4444;
        margin-left: 4px;
    }

    /* ACTIVE */
    .active-group {
        display: flex;
        flex-direction: column;
        justify-content: flex-end;
    }

    .checkbox-pill {
        display: inline-flex;
        align-items: center;
        gap: 10px;
        padding: 10px 14px;
        border: 1px solid #d1d5db;
        border-radius: 999px;
        background: #f9fafb;
        width: fit-content;
        cursor: pointer;
    }

        .checkbox-pill input {
            width: 16px;
            height: 16px;
            margin: 0;
        }

    /* IMAGE */
    .file-hidden {
        display: none;
    }

    .image-uploader {
        border: 1px solid #e5e7eb;
        border-radius: 16px;
        background: #ffffff;
        padding: 14px;
        box-shadow: 0 10px 26px rgba(15, 23, 42, 0.06);
    }

    .image-empty {
        display: flex;
        gap: 12px;
        align-items: center;
        padding: 14px;
        border: 1px dashed #cbd5e1;
        border-radius: 14px;
        background: #f8fafc;
    }

    .img-icon {
        width: 44px;
        height: 44px;
        border-radius: 12px;
        display: grid;
        place-items: center;
        background: rgba(67, 40, 93, 0.08);
        font-size: 20px;
    }

    .img-title {
        font-weight: 650;
        color: #111827;
    }

    .img-sub {
        font-size: 12px;
        color: #6b7280;
        margin-top: 2px;
    }

    .image-preview {
        position: relative;
        border-radius: 14px;
        overflow: hidden;
        border: 1px solid #e5e7eb;
        background: #f8fafc;
    }

        .image-preview img {
            width: 100%;
            height: 220px;
            object-fit: cover;
            display: block;
        }

    .img-remove {
        position: absolute;
        top: 10px;
        right: 10px;
        border: none;
        cursor: pointer;
        width: 34px;
        height: 34px;
        border-radius: 999px;
        background: rgba(255, 255, 255, 0.92);
        box-shadow: 0 6px 14px rgba(0, 0, 0, 0.15);
        font-size: 16px;
    }

    .img-actions {
        display: flex;
        gap: 10px;
        margin-top: 12px;
    }

    .btn-upload {
        background: #43285D;
        color: #fff;
        border: none;
        border-radius: 999px;
        padding: 10px 16px;
        font-weight: 650;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(67, 40, 93, 0.22);
        transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

        .btn-upload:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 18px rgba(67, 40, 93, 0.28);
        }

        .btn-upload.subtle {
            background: #eef2f7;
            color: #111827;
            box-shadow: none;
            border: 1px solid #dbe2ea;
        }

            .btn-upload.subtle:hover {
                background: #e6ebf2;
                transform: translateY(-2px);
            }

    .error-text {
        display: block;
        margin-top: 8px;
        color: #ef4444;
        font-size: 12px;
    }

    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary,
    .btn-secondary {
        transition: all 0.25s ease;
        transform: translateY(0);
    }

    .btn-primary {
        background: #43285D;
        color: white;
        padding: 10px 22px;
        border: none;
        border-radius: 999px;
        font-weight: 600;
        box-shadow: 0 4px 12px rgba(67, 40, 93, 0.22);
        cursor: pointer;
    }

        .btn-primary:hover {
            background: #361F4A;
            transform: translateY(-3px);
            box-shadow: 0 8px 18px rgba(67, 40, 93, 0.32);
        }

    .btn-secondary {
        background: #e5e7eb;
        color: #333;
        padding: 10px 22px;
        border-radius: 999px;
        font-weight: 500;
        box-shadow: 0 2px 6px rgba(0,0,0,0.10);
        border: none;
        cursor: pointer;
    }

        .btn-secondary:hover {
            background: #d4d4d4;
            transform: translateY(-3px);
            box-shadow: 0 6px 14px rgba(0,0,0,0.15);
        }
    .quill-box.resizable {
        min-height: 220px;
        border-radius: 12px;
        overflow: hidden;
        background: #fff;
    }

    .quill-box :deep(.ql-toolbar) {
        border: 1px solid #d1d5db;
        border-bottom: none;
        background: #f8fafc;
        border-top-left-radius: 12px;
        border-top-right-radius: 12px;
    }

    .quill-box :deep(.ql-container) {
        border: 1px solid #d1d5db;
        border-bottom-left-radius: 12px;
        border-bottom-right-radius: 12px;
        min-height: 160px;
        font-size: 14px;
        background: #ffffff;
    }

    .quill-box :deep(.ql-editor) {
        min-height: 160px;
        line-height: 1.7;
        color: #374151;
    }
</style>