<template>
    <div class="modal-overlay">
        <div class="modal fade-in">

            <!-- Close -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- PURPLE HEADER (MATCHES CREATE & SCHEDULE) -->
            <header class="modal-header">
                <div>
                    <h2>Edit Training Title</h2>
                    <p class="modal-subtitle">Modify course metadata, credits, category & certificate details.</p>
                </div>
                <span class="modal-badge">Admin • Title Editor</span>
            </header>

            <!-- Body -->
            <form @submit.prevent="updateTitle">

                <!-- BASIC INFO -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Basic Information</h3>
                        <p>Edit title name and primary metadata.</p>
                    </div>

                    <!-- TITLE IMAGE -->
                    <section class="section-card">
                        <div class="section-header">
                            <h3>Title Image</h3>
                            <p>Upload or replace the banner image for this training title.</p>
                        </div>

                        <div class="image-grid">
                            <!-- Current image -->
                            <div class="image-card">
                                <div class="image-card-head">
                                    <span class="pill">Current</span>
                                    <button v-if="hasCurrentImage"
                                            type="button"
                                            class="link-btn"
                                            @click="refreshCurrentImage"
                                            title="Refresh">
                                        Refresh
                                    </button>
                                </div>

                                <div class="image-preview">
                                    <img v-if="hasCurrentImage"
                                         :src="currentImageUrl"
                                         alt="Current title image" />
                                    <div v-else class="image-empty">
                                        No image uploaded yet
                                    </div>
                                </div>
                            </div>

                            <!-- New image -->
                            <div class="image-card">
                                <div class="image-card-head">
                                    <span class="pill pill-soft">New</span>
                                    <button v-if="newImageFile"
                                            type="button"
                                            class="link-btn danger"
                                            @click="clearNewImage"
                                            title="Remove selected image">
                                        Remove
                                    </button>
                                </div>

                                <div class="dropzone"
                                     :class="{ 'dropzone-has-file': !!newImageFile }"
                                     @dragover.prevent
                                     @drop.prevent="onDropImage">
                                    <input ref="fileInput"
                                           type="file"
                                           accept="image/png,image/jpeg,image/webp"
                                           class="file-input-hidden"
                                           @change="onPickImage" />

                                    <div v-if="!newImagePreviewUrl" class="dropzone-content">
                                        <div class="icon">🖼️</div>
                                        <div class="title">Drag & drop an image here</div>
                                        <div class="sub">PNG / JPG / WEBP • Max 2MB</div>

                                        <button type="button" class="btn-upload" @click="openFilePicker">
                                            Choose Image
                                        </button>
                                    </div>

                                    <div v-else class="new-preview">
                                        <img :src="newImagePreviewUrl" alt="New preview" />
                                        <div class="new-meta">
                                            <div class="file-name">{{ newImageFile?.name }}</div>
                                            <div class="file-sub">
                                                {{ (newImageFile?.size / 1024).toFixed(0) }} KB
                                            </div>

                                            <button type="button" class="btn-upload ghost" @click="openFilePicker">
                                                Change Image
                                            </button>
                                        </div>
                                    </div>
                                </div>

                                <small class="hint" v-if="imageError" style="color:#ef4444;">
                                    {{ imageError }}
                                </small>
                            </div>
                        </div>
                    </section>

                    <div class="form-group">
                        <label>Course Title *</label>
                        <input v-model="form.courseTitle" required />
                    </div>
                </section>

                <!-- DESCRIPTION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Description</h3>
                        <p>Update detailed descriptive content.</p>
                    </div>

                    <quill-editor v-model:content="form.description"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box" />
                </section>

                <!-- TOPICS / CREDITS -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Topic & Credits</h3>
                        <p>Manage title topics and related credit types.</p>
                    </div>

                    <div class="form-group">
                        <label>
                            Topics <span class="required">*</span>
                        </label>

                        <div class="topic-multi" :class="{ 'required-border': topicError }">
                            <label v-for="t in topics" :key="t.code" class="topic-item">
                                <input type="checkbox" :value="t.code" v-model="form.topicCodes" />
                                <span>{{ t.value }}</span>
                            </label>
                        </div>

                        <small v-if="topicError" class="error-text">
                            Please select at least one topic.
                        </small>

                        <small class="hint" v-else-if="form.topicCodes.length">
                            Selected: {{ form.topicCodes.length }}
                        </small>
                    </div>

                    <div class="checkbox-row">
                        <label><input type="checkbox" v-model="form.cnecredits" /> CNE Hours</label>
                        <label><input type="checkbox" v-model="form.oasascredits" /> OASAS Hours</label>
                    </div>
                </section>

                <!-- CERTIFICATE -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Certificate</h3>
                        <p>Edit certificate description & notes.</p>
                    </div>

                    <quill-editor v-model:content="form.certDescription"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box" />

                    <div class="form-group">
                        <label>Certificate Notes</label>
                        <textarea v-model="form.miscCertDesc" class="auto-expand"></textarea>
                    </div>
                </section>

                <!-- ONLINE SECTION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Delivery Format</h3>
                        <p>Specify online training details & URLs.</p>
                    </div>

                    <div class="form-group">
                        <label>Is it an Online Training?</label>
                        <div class="radio-group">
                            <label class="radio-item">
                                <input type="radio" value="true" v-model="form.isOnlineTraining" />
                                <span>Yes</span>
                            </label>

                            <label class="radio-item">
                                <input type="radio" value="false" v-model="form.isOnlineTraining" />
                                <span>No</span>
                            </label>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>
                            WebCast / Online Training URL
                            <span v-if="form.isOnlineTraining === 'true'" class="required">*</span>
                        </label>
                        <input v-model="form.videoUrl"
                               :required="form.isOnlineTraining === 'true'"
                               :class="{ 'required-border': form.isOnlineTraining === 'true' && !form.videoUrl }" />
                    </div>

                    <div class="form-group" v-if="form.isOnlineTraining === 'true'">
                        <label>Mark as New Until (optional)</label>
                        <input type="date" v-model="form.markAsNewUntil" class="date-input" />
                    </div>
                </section>

                <!-- BUTTONS -->
                <div class="button-group">
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                    <button type="submit" class="btn-primary">Update</button>
                </div>

            </form>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios';
    import { QuillEditor } from '@vueup/vue-quill';

    export default {
        components: { QuillEditor },
        props: ['title'],
        emits: ['close', 'updated'],
        data() {
            return {
                form: {
                    subjectSysId: null,
                    courseTitle: '',
                    description: '',
                            topicCodes: [], // ✅ NEW

                   // category: '',
                    cnecredits: false,
                    oasascredits: false,
                    certDescription: '',
                    miscCertDesc: '',
                    videoUrl: '',
                    isOnlineTraining: 'false',
                    markAsNewUntil: null
                },
                topics: [],
      topicError: false,

      //  image state
  hasCurrentImage: false,
  currentImageUrl: "",
  newImageFile: null,
  newImagePreviewUrl: "",
  imageError: ""
            };
        },
        async mounted() {
            const res = await apiClient.get("/Lookup/topics");
    this.topics = Array.isArray(res.data) ? res.data : (res.data?.$values || []);

    const t = await apiClient.get(`/TrainingTitle/${this.title.subjectSysId}`);
    const data = t.data;

    const normalizeArray = (v) => {
  if (Array.isArray(v)) return v;
  if (v && Array.isArray(v.$values)) return v.$values;
  return [];
};

    const topicCodesFromApi =
  normalizeArray(data.topicCodes).length
    ? normalizeArray(data.topicCodes)
    : normalizeArray(data.topics).map(x => x.code);

            this.form = {
                subjectSysId: data.subjectSysId,
                courseTitle: data.courseTitle ?? '',
                description: data.description ?? '',
  topicCodes: topicCodesFromApi.map(Number).filter(n => !Number.isNaN(n)),
                cnecredits: data.cnecredits ?? false,
                oasascredits: data.oasascredits ?? false,
                certDescription: data.certDescription ?? '',
                miscCertDesc: data.miscCertDesc ?? '',
                videoUrl: data.videoUrl ?? '',
                isOnlineTraining: data.isOnlineTraining ? 'true' : 'false',
                markAsNewUntil: data.markAsNewUntil
                    ? data.markAsNewUntil.substring(0, 10)
                    : null
            };
this.hasCurrentImage = !!data.hasTitleImage || !!data.titleImagePath;

// set image url if available
if (this.hasCurrentImage) {
  this.refreshCurrentImage();
}
        },
        methods: {

        openFilePicker() {
  this.$refs.fileInput?.click();
},

refreshCurrentImage() {
  // cache bust to force reload after upload
  const id = this.form.subjectSysId;
  this.currentImageUrl = `/api/TrainingTitle/${id}/image?t=${Date.now()}`;
},

clearNewImage() {
  this.newImageFile = null;
  this.newImagePreviewUrl = "";
  this.imageError = "";
  if (this.$refs.fileInput) this.$refs.fileInput.value = "";
},

validateImageFile(file) {
  this.imageError = "";

  if (!file) return false;

  const allowed = ["image/png", "image/jpeg", "image/webp"];
  if (!allowed.includes(file.type)) {
    this.imageError = "Invalid image type. Allowed: PNG / JPG / WEBP.";
    return false;
  }

  const maxBytes = 2 * 1024 * 1024;
  if (file.size > maxBytes) {
    this.imageError = "Image too large. Max size is 2MB.";
    return false;
  }

  return true;
},

setNewImage(file) {
  if (!this.validateImageFile(file)) return;

  this.newImageFile = file;
  this.newImagePreviewUrl = URL.createObjectURL(file);
},

onPickImage(e) {
  const file = e.target.files?.[0];
  if (!file) return;
  this.setNewImage(file);
},

onDropImage(e) {
  const file = e.dataTransfer?.files?.[0];
  if (!file) return;
  this.setNewImage(file);
},
            async updateTitle() {
  try {
    // ✅ TOPIC REQUIRED
    const topicCodes = (this.form.topicCodes || [])
      .map(Number)
      .filter(n => !Number.isNaN(n));

    if (topicCodes.length === 0) {
      this.topicError = true;
      alert("Please select at least one topic.");
      return;
    }
    this.topicError = false;

    // ✅ Online URL required if online
    if (this.form.isOnlineTraining === "true" && !this.form.videoUrl?.trim()) {
      alert("Please provide a WebCast or Online Training URL.");
      return;
    }

    // ✅ 1) Update title data
    const payload = {
      subjectSysId: this.form.subjectSysId,
      courseTitle: this.form.courseTitle,
      description: this.form.description || null,
      topicCodes,
      cnecredits: this.form.cnecredits,
      oasascredits: this.form.oasascredits,
      certDescription: this.form.certDescription || null,
      miscCertDesc: this.form.miscCertDesc || null,
      videoUrl: this.form.videoUrl || null,
      isOnlineTraining: this.form.isOnlineTraining === "true",
      markAsNewUntil: this.form.markAsNewUntil || null
    };

    await apiClient.put(`/TrainingTitle/update/${this.form.subjectSysId}`, payload);

    // ✅ 2) If new image selected, upload it
    if (this.newImageFile) {
      const fd = new FormData();
      fd.append("file", this.newImageFile);

      await apiClient.post(`/TrainingTitle/${this.form.subjectSysId}/image`, fd, {
        headers: { "Content-Type": "multipart/form-data" }
      });

      // update UI current image immediately
      this.hasCurrentImage = true;
      this.refreshCurrentImage();
      this.clearNewImage();
    }

    alert("Training title updated successfully!");
    this.$emit("updated");
    this.$emit("close");
  } catch (err) {
    console.error("Error updating title", err?.response?.data || err);
    alert(err?.response?.data?.message || "Failed to update training title.");
  }
},
        },
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
        z-index: 999;
    }

    /* MODAL */
    .modal {
        position: relative;
        background: #ffffff;
        border-radius: 24px;
        width: 1100px;
        max-height: 90vh;
        overflow-y: auto;
        padding: 32px;
        box-shadow: 0 24px 60px rgba(15,23,42,0.25), 0 0 0 1px rgba(148,163,184,0.35);
        font-family: system-ui, "Segoe UI", sans-serif;
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
        cursor: pointer;
        font-size: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
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

    .modal-badge {
        background: rgba(255,255,255,0.18);
        color: white;
        padding: 8px 20px;
        border-radius: 999px;
        font-size: 12px;
        font-weight: 600;
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

    /* INPUTS */
    input, select, textarea {
        width: 100%;
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
    }

    textarea {
        resize: vertical;
        min-height: 110px;
    }

    /* QUILL EDITOR RESIZABLE */
    .quill-box {
        border: 1px solid #d1d5db;
        border-radius: 12px;
        min-height: 150px;
        resize: vertical;
        overflow: auto;
    }

    /* RADIO BUTTONS (MATCH CREATE TITLE) */
    .radio-group {
        display: flex;
        gap: 16px;
        margin-top: 8px;
    }

    .radio-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        border: 1px solid #d1d5db;
        border-radius: 999px;
        background: #f9fafb;
        cursor: pointer;
        transition: all 0.2s ease;
    }

        .radio-item:hover {
            background: #f1eef7;
            border-color: #43285D;
        }

        .radio-item input[type="radio"] {
            accent-color: #43285D;
            width: 16px;
            height: 16px;
        }

        .radio-item input:checked + span {
            color: #43285D;
            font-weight: 600;
        }

    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary {
        background: #43285D;
        padding: 10px 22px;
        color: white;
        border: none;
        border-radius: 999px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.25s ease;
    }

        .btn-primary:hover {
            background: #341F49;
        }

    .btn-secondary {
        background: #e5e7eb;
        padding: 10px 22px;
        border-radius: 999px;
        cursor: pointer;
        transition: background 0.25s ease;
    }

        .btn-secondary:hover {
            background: #d5d5d5;
        }

    /* REQUIRED */
    .required {
        color: red;
    }

    .required-border {
        border-color: red !important;
    }
    .radio-group {
        display: flex;
        gap: 16px;
        margin-top: 8px;
    }

    .radio-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        border: 1px solid #d1d5db;
        border-radius: 999px;
        background: #f9fafb;
        cursor: pointer;
        transition: all 0.2s ease;
    }

        .radio-item:hover {
            background: #f1eef7;
            border-color: #43285D;
        }

        .radio-item input[type="radio"] {
            accent-color: #43285D;
            width: 16px;
            height: 16px;
        }

        .radio-item input:checked + span {
            color: #43285D;
            font-weight: 600;
        }
    .topic-multi {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 10px;
        margin-top: 8px;
    }

    .topic-item {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 10px 12px;
        border: 1px solid #d1d5db;
        border-radius: 10px;
        background: #f9fafb;
        cursor: pointer;
    }

        .topic-item input {
            width: 16px;
            height: 16px;
        }

    .hint {
        display: block;
        margin-top: 8px;
        opacity: 0.8;
        font-size: 12px;
    }

    .required {
        color: #ef4444;
        margin-left: 4px;
    }

    .required-border {
        border: 1px solid #ef4444 !important;
        border-radius: 10px;
        padding: 10px;
    }

    .error-text {
        display: block;
        margin-top: 8px;
        color: #ef4444;
        font-size: 12px;
    }
    .image-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 16px;
    }

    @media (max-width: 900px) {
        .image-grid {
            grid-template-columns: 1fr;
        }
    }

    .image-card {
        border: 1px solid #e5e7eb;
        border-radius: 16px;
        background: #fff;
        overflow: hidden;
        box-shadow: 0 10px 24px rgba(15,23,42,0.06);
    }

    .image-card-head {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 12px 14px;
        border-bottom: 1px solid #f1f5f9;
    }

    .pill {
        font-size: 12px;
        font-weight: 700;
        color: #43285D;
        background: rgba(67,40,93,0.12);
        padding: 6px 10px;
        border-radius: 999px;
    }

    .pill-soft {
        background: rgba(59,130,246,0.12);
        color: #1f4b99;
    }

    .link-btn {
        border: none;
        background: transparent;
        color: #43285D;
        font-weight: 600;
        cursor: pointer;
        padding: 6px 8px;
        border-radius: 10px;
    }

        .link-btn:hover {
            background: rgba(67,40,93,0.08);
        }

        .link-btn.danger {
            color: #b91c1c;
        }

            .link-btn.danger:hover {
                background: rgba(185,28,28,0.10);
            }

    .image-preview {
        padding: 14px;
        height: 210px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #f8fafc;
    }

        .image-preview img {
            max-width: 100%;
            max-height: 100%;
            border-radius: 12px;
            border: 1px solid #e5e7eb;
            background: #fff;
        }

    .image-empty {
        color: #6b7280;
        font-size: 13px;
    }

    .file-input-hidden {
        display: none;
    }

    .dropzone {
        padding: 14px;
        height: 210px;
        background: #f8fafc;
        display: flex;
        align-items: center;
        justify-content: center;
        border-top: 1px solid #f1f5f9;
    }

    .dropzone-content {
        text-align: center;
    }

    .dropzone .icon {
        font-size: 26px;
        margin-bottom: 8px;
    }

    .dropzone .title {
        font-weight: 700;
        color: #111827;
    }

    .dropzone .sub {
        margin-top: 4px;
        font-size: 12px;
        color: #6b7280;
    }

    .dropzone-has-file {
        background: #ffffff;
    }

    .new-preview {
        display: flex;
        gap: 12px;
        align-items: center;
        width: 100%;
    }

        .new-preview img {
            width: 160px;
            height: 96px;
            object-fit: cover;
            border-radius: 12px;
            border: 1px solid #e5e7eb;
        }

    .new-meta {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

    .file-name {
        font-weight: 700;
        color: #111827;
        max-width: 260px;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .file-sub {
        font-size: 12px;
        color: #6b7280;
    }

    .btn-upload {
        margin-top: 12px;
        background: #43285D;
        color: white;
        border: none;
        border-radius: 999px;
        padding: 10px 16px;
        font-weight: 700;
        cursor: pointer;
        box-shadow: 0 8px 18px rgba(67,40,93,0.18);
        transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

        .btn-upload:hover {
            transform: translateY(-2px);
            box-shadow: 0 14px 26px rgba(67,40,93,0.24);
        }

        .btn-upload.ghost {
            background: #eef2ff;
            color: #43285D;
            box-shadow: none;
        }

            .btn-upload.ghost:hover {
                background: #e0e7ff;
                transform: translateY(-1px);
            }
</style>