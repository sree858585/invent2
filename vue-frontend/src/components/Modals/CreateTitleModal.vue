<template>
    <div class="modal-overlay">
        <div class="modal fade-in">
            <!-- Close -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- Header -->
            <header class="modal-header">
                <div>
                    <h2>Create New Training Title</h2>
                    <p class="modal-subtitle">
                        Define course metadata, credits, category & certificate details.
                    </p>
                </div>
                <span class="modal-badge">Admin • Title Creator</span>
            </header>

            <form @submit.prevent="submitTitle">
                <!-- BASIC INFORMATION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Basic Information</h3>
                        <p>Enter the main details for this title.</p>
                    </div>

                    <div class="form-group">
                        <label>Course Title <span class="required">*</span></label>
                        <input v-model.trim="form.courseTitle" placeholder="Enter course title" required />
                    </div>

                    <!-- IMAGE UPLOAD (OPTIONAL) -->
                    <div class="form-group">
                        <label>Title Image <span class="muted">(optional)</span></label>

                        <div class="image-uploader">
                            <input ref="fileInput"
                                   class="file-hidden"
                                   type="file"
                                   accept="image/png,image/jpeg,image/webp"
                                   @change="onImageSelected" />

                            <div class="image-preview" v-if="imagePreviewUrl">
                                <img :src="imagePreviewUrl" alt="Title preview" />
                                <button type="button" class="img-remove" @click="clearImage" title="Remove image">
                                    ✕
                                </button>
                            </div>

                            <div class="image-empty" v-else>
                                <div class="img-icon">🖼️</div>
                                <div class="img-text">
                                    <div class="img-title">Upload a banner image</div>
                                    <div class="img-sub">PNG/JPG/WebP • Max 2MB • Recommended 1600×600</div>
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

                        <small class="hint" v-if="imageError" style="color:#ef4444;">
                            {{ imageError }}
                        </small>
                    </div>
                </section>

                <!-- DESCRIPTION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Description</h3>
                        <p>Enter detailed content for this course title.</p>
                    </div>

                    <quill-editor v-model:content="form.description"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box resizable" />
                </section>

                <!-- CATEGORY & CREDITS -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Topic & Credits</h3>
                        <p>Choose category and credit types applicable.</p>
                    </div>

                    <div class="form-group">
                        <!--<label>Category</label>
    <select v-model="form.category">
        <option value="">-- Select Category --</option>
        <option v-for="cat in categories" :key="cat.code" :value="String(cat.code)">
            {{ cat.value }}
        </option>
    </select>-->
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
                        <h3>Certificate Details</h3>
                        <p>Add certificate description and notes.</p>
                    </div>

                    <label>Certificate Description</label>
                    <quill-editor v-model:content="form.certDescription"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box resizable" />

                    <div class="form-group">
                        <label>Certificate Notes</label>
                        <textarea v-model="form.miscCertDesc"
                                  class="resizable-textarea"
                                  placeholder="Enter special notes"></textarea>
                    </div>
                </section>

                <!-- ONLINE TRAINING -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Delivery Format</h3>
                        <p>Specify whether this title is online.</p>
                    </div>

                    <div class="form-group">
                        <label>Is it an Online Training?</label>
                        <div class="radio-group">
                            <label class="radio-item">
                                <input type="radio" :value="true" v-model="form.isOnlineTraining" />
                                <span>Yes</span>
                            </label>
                            <label class="radio-item">
                                <input type="radio" :value="false" v-model="form.isOnlineTraining" />
                                <span>No</span>
                            </label>
                        </div>
                    </div>

                    <div class="form-group" v-if="form.isOnlineTraining">
                        <label>
                            SCORM Package ZIP <span class="required">*</span>
                        </label>

                        <input ref="scormInput"
                               type="file"
                               accept=".zip,application/zip,application/x-zip-compressed"
                               @change="onScormSelected"
                               :class="{ 'required-border': form.isOnlineTraining && !scormZipFile }" />

                        <small class="hint">
                            Upload the SCORM ZIP package. The system will store and prepare it for launch.
                        </small>

                        <small v-if="scormError" class="error-text">
                            {{ scormError }}
                        </small>
                    </div>

                    <div class="form-group" v-if="form.isOnlineTraining">
                        <label>Mark as New Until (optional)</label>
                        <input type="date" v-model="form.markAsNewUntil" />
                    </div>
                </section>

                <!-- BUTTONS -->
                <div class="button-group">
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                    <button type="submit" class="btn-primary">Create</button>
                </div>
            </form>
        </div>
    </div>
</template>
<script>import apiClient from "@/axios";
import { QuillEditor } from "@vueup/vue-quill";

export default {
  components: { QuillEditor },
  emits: ["close", "created"],
  data() {
  return {
    form: {
      courseTitle: "",
      description: "",
      topicCodes: [],
      cnecredits: false,
      oasascredits: false,
      certDescription: "",
      miscCertDesc: "",
      videoUrl: "",
      isOnlineTraining: false,
      markAsNewUntil: null,
    },

    topics: [],
    topicError: false,

    titleImageFile: null,
    imagePreviewUrl: "",
    imageError: "",

    scormZipFile: null,
    scormError: "",
  };
},

  async mounted() {
    const res = await apiClient.get("/Lookup/topics");
    this.topics = Array.isArray(res.data) ? res.data : (res.data?.$values || []);
  },

  beforeUnmount() {
    // cleanup objectURL
    if (this.imagePreviewUrl?.startsWith("blob:")) {
      URL.revokeObjectURL(this.imagePreviewUrl);
    }
  },

  methods: {
   triggerFilePicker() {
  if (this.$refs.fileInput) {
    this.$refs.fileInput.value = null; 
  }
  this.$refs.fileInput?.click();
},
onScormSelected(e) {
    this.scormError = "";
    const file = e.target.files?.[0] || null;

    if (!file) {
        this.scormZipFile = null;
        return;
    }

    const isZip =
        file.name.toLowerCase().endsWith(".zip") ||
        file.type === "application/zip" ||
        file.type === "application/x-zip-compressed";

    if (!isZip) {
        this.scormError = "Please upload a valid SCORM ZIP file.";
        this.scormZipFile = null;
        if (this.$refs.scormInput) this.$refs.scormInput.value = "";
        return;
    }

    const maxBytes = 200 * 1024 * 1024; // 200MB
    if (file.size > maxBytes) {
        this.scormError = "SCORM package is too large. Max allowed is 200MB.";
        this.scormZipFile = null;
        if (this.$refs.scormInput) this.$refs.scormInput.value = "";
        return;
    }

    this.scormZipFile = file;
},

    onImageSelected(e) {
      this.imageError = "";
      const file = e.target.files?.[0] || null;
      if (!file) return;

      // ✅ Validate size (2MB) and type
      const allowed = ["image/png", "image/jpeg", "image/webp"];
      if (!allowed.includes(file.type)) {
        this.imageError = "Invalid image type. Please upload PNG/JPG/WebP.";
        this.clearImage();
        return;
      }
      const maxBytes = 2 * 1024 * 1024;
      if (file.size > maxBytes) {
        this.imageError = "Image too large. Max allowed is 2MB.";
        this.clearImage();
        return;
      }

      // ✅ set file + preview
      this.titleImageFile = file;

      if (this.imagePreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(this.imagePreviewUrl);
      }
      this.imagePreviewUrl = URL.createObjectURL(file);
    },

    clearImage() {
      this.titleImageFile = null;

      if (this.imagePreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(this.imagePreviewUrl);
      }
      this.imagePreviewUrl = "";
      this.imageError = "";

      // reset input
      if (this.$refs.fileInput) this.$refs.fileInput.value = "";
    },

    async submitTitle() {
      try {
        // ✅ TOPIC REQUIRED
        const topicCodes = (this.form.topicCodes || [])
          .map(Number)
          .filter((n) => !Number.isNaN(n));

        if (topicCodes.length === 0) {
          this.topicError = true;
          alert("Please select at least one topic.");
          return;
        }
        this.topicError = false;

        // ✅ Online training URL required if online
        if (this.form.isOnlineTraining && !this.scormZipFile) {
    alert("Please upload a SCORM ZIP package.");
    return;
}

        // 1) Create Title (JSON)
        const payload = {
          courseTitle: this.form.courseTitle,
          description: this.form.description || null,
          topicCodes,
          cnecredits: this.form.cnecredits,
          oasascredits: this.form.oasascredits,
          certDescription: this.form.certDescription || null,
          miscCertDesc: this.form.miscCertDesc || null,
          videoUrl: this.form.videoUrl || null,
          isOnlineTraining: this.form.isOnlineTraining,
          markAsNewUntil: this.form.markAsNewUntil || null,
        };

        const res = await apiClient.post("/TrainingTitle/create", payload);
        const subjectId = res.data?.subjectId;

        // 2) Upload image if selected
        if (subjectId && this.titleImageFile) {
          const fd = new FormData();
          fd.append("file", this.titleImageFile);

          await apiClient.post(`/TrainingTitle/${subjectId}/image`, fd, {
            headers: { "Content-Type": "multipart/form-data" },
          });
        }

        if (subjectId && this.form.isOnlineTraining && this.scormZipFile) {
    const scormFd = new FormData();
    scormFd.append("file", this.scormZipFile);

    await apiClient.post(`/TrainingTitle/${subjectId}/scorm-package`, scormFd, {
        headers: { "Content-Type": "multipart/form-data" },
    });
}

        alert(res.data?.message || "Training title created successfully!");
        this.$emit("created");
        this.$emit("close");
      } catch (err) {
        console.error("Error creating title", err?.response?.data || err);
        alert(err?.response?.data?.message || "Failed to create title.");
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
    }

    /* PURPLE HEADER */
    .modal-header {
        background: #43285D; /* Final purple */
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

    /* FORM */
    input, select, textarea {
        width: 100%;
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
        font-size: 14px;
    }

        textarea.resizable-textarea {
            min-height: 120px;
            resize: vertical;
        }

    /* QUILL EDITOR — RESIZABLE */
    .quill-box.resizable {
        min-height: 200px;
        resize: vertical;
        overflow: auto;
        border-radius: 12px;
    }

    /* RADIO BUTTONS */
    .radio-group {
        display: flex;
        gap: 20px;
        margin-top: 6px;
    }

    .radio-item {
        display: flex;
        align-items: center;
        gap: 6px;
        padding: 8px 16px;
        border: 1px solid #d1d5db;
        border-radius: 10px;
        background: #fafafa;
        cursor: pointer;
    }

        .radio-item input {
            width: 16px;
            height: 16px;
        }

    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary {
        background: #43285D; /* Apply purple here */
        color: white;
        padding: 10px 22px;
        border: none;
        border-radius: 999px;
        font-weight: 600;
    }

        .btn-primary:hover {
            background: #361F4A;
        }

    .btn-secondary {
        background: #e5e7eb;
        padding: 10px 22px;
        border-radius: 999px;
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

    /* PRIMARY BUTTON */
    .btn-primary {
        background: #43285D;
        color: white;
        padding: 10px 22px;
        border: none;
        border-radius: 999px;
        font-weight: 600;
        box-shadow: 0 4px 12px rgba(67, 40, 93, 0.22);
    }

        .btn-primary:hover {
            background: #361F4A;
            transform: translateY(-3px);
            box-shadow: 0 8px 18px rgba(67, 40, 93, 0.32);
        }

    /* SECONDARY BUTTON */
    .btn-secondary {
        background: #e5e7eb;
        color: #333;
        padding: 10px 22px;
        border-radius: 999px;
        font-weight: 500;
        box-shadow: 0 2px 6px rgba(0,0,0,0.10);
    }

        .btn-secondary:hover {
            background: #d4d4d4;
            transform: translateY(-3px);
            box-shadow: 0 6px 14px rgba(0,0,0,0.15);
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
    /* ===== Image Upload (Create Title) ===== */
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
            height: 180px;
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

    .muted {
        color: #6b7280;
        font-weight: 500;
        font-size: 12px;
    }
</style>