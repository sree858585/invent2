<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal-content">
            <!-- Title -->
            <div class="modal-header">
                <h2>Cancel Course</h2>
                <button class="close-btn" @click="$emit('close')">&times;</button>
            </div>

            <!-- Course Summary -->
            <div class="course-summary">
                <div><strong>Training Center:</strong> {{ course.siteName }}</div>
                <div><strong>Region:</strong> {{ course.regionLabel }}</div>
                <div><strong>Category:</strong> {{ course.categoryLabel }}</div>
                <div><strong>Course Title:</strong> {{ course.subjectTitle }}</div>
                <div><strong>1st Instructor:</strong> {{ course.instructorLabel }}</div>
                <div><strong>2nd Instructor:</strong> {{ course.instructor2Label || 'N/A' }}</div>
                <div><strong>Course Start Date:</strong> {{ formatDate(course.courseDate) }}</div>
                <div><strong>Course End Date:</strong> {{ formatDate(course.courseEndDate) }}</div>
                <div><strong>Registered:</strong> {{ course.registeredCount }}</div>
                <div><strong>Maximum Seats:</strong> {{ course.maxSeats }}</div>
                <div><strong>Training Center Location:</strong> {{ course.trainingLocation }}</div>
                <div><strong>Funding Type:</strong> {{ course.fundingType || 'N/A' }}</div>
            </div>

            <!-- Email Message Box -->
            <div class="cancel-reason-box">
                <label class="email-label">Email that will go out to all registered users</label>
                <quill-editor v-model:content="cancelReason"
                              contentType="html"
                              theme="snow"
                              placeholder="Enter email content here..."
                              class="quill-box" />
            </div>


            <!-- Actions -->
            <div class="button-group">
                <button class="btn-primary" @click="submitCancellation">Cancel course and email registered users</button>
                <button class="btn-secondary" @click="$emit('close')">Cancel</button>
            </div>
        </div>
    </div>

    <!-- Confirmation Modal -->
    <div v-if="showConfirmDialog" class="modal-overlay">
        <div class="modal confirmation">
            <h3>Confirm Cancellation</h3>
            <p>Are you sure you want to cancel <strong>{{ course?.subjectTitle }}</strong>?</p>
            <div class="button-group">
                <button class="btn-danger" @click="confirmCancellation">Yes, Cancel</button>
                <button class="btn-secondary" @click="showConfirmDialog = false">No</button>
            </div>
        </div>
    </div>

    <!-- Success Modal -->
    <div v-if="showSuccessDialog" class="modal-overlay">
        <div class="modal success">
            <h3>Course Cancelled</h3>
            <p>The course <strong>{{ course?.subjectTitle }}</strong> was successfully cancelled and all registered users have been notified.</p>
            <div class="button-group">
                <button class="btn-primary" @click="closeSuccessDialog">Close</button>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios';
import { QuillEditor } from '@vueup/vue-quill'


export default {
  components: { QuillEditor },
  props: ['course'],
  data() {
    return {
    cancelReason: '',
    registeredCount: 0,
    showConfirmDialog: false,
    showSuccessDialog: false
  };
  },

  computed: {
  cancelEmailContent() {
  const { subjectTitle, courseDate, courseEndDate, siteName, trainingLocation } = this.course;

  const start = courseDate ? new Date(courseDate).toLocaleDateString("en-US") : "N/A";
  const end = courseEndDate ? new Date(courseEndDate).toLocaleDateString("en-US") : "N/A";

  return `
    <p>Dear Registrant,</p>

    <p>The following New York State Department of Health AIDS Institute course <strong>has been cancelled</strong>:</p>

    <p><strong>Course Title:</strong> ${subjectTitle}</p>
    <p><strong>Course Start Date:</strong> ${start}</p>
    <p><strong>Course End Date:</strong> ${end}</p>
    <p><strong>Course Schedule:</strong> 9:00–4:00 both days</p>
    <p><strong>PLEASE NOTE:</strong> If you miss more than 10 minutes of any part of the course, you will not be marked as "attended".</p>
    <p><strong>Course Location:</strong> ${trainingLocation}</p>

    <p>Please call ${siteName} for more information.</p>

    <p>Thank you.</p>
  `;
}
},
  mounted() {
  console.log("Cancel modal mounted for course:", this.course?.courseSysId);
  this.cancelReason = this.cancelEmailContent; // 👈 Default HTML text for Quill
  this.fetchRegisteredCount();
},
  methods: {
    formatDate(date) {
  if (!date) return "N/A";
  const d = new Date(date);
  return isNaN(d.getTime()) ? "N/A" : d.toLocaleDateString();
},
    async fetchRegisteredCount() {
  if (!this.course?.courseSysId) {
    console.warn("Course ID missing");
    return;
  }

  try {
    const res = await apiClient.get('/CourseAdmin/registered-user-ids', {
      params: { courseId: this.course.courseSysId }
    });
    console.log("✅ API response:", res.data);  // Log the full response
    this.registeredCount = res.data.length;
  } catch (err) {
    console.error("❌ Failed to fetch registered count:", err);
  }
},
async confirmCancellation() {
  try {
    const response = await apiClient.post('/Email/cancel-course', {
      courseId: this.course.courseSysId,
      message: this.cancelReason
    });

    console.log(
      "Course cancellation response:",
      response.data
    );

    this.showConfirmDialog = false;
    this.showSuccessDialog = true;

  } catch (error) {
    console.error(
      'Cancellation failed:',
      error
    );

    alert(
      error.response?.data?.message ||
      'An error occurred while cancelling the course.'
    );

    this.showConfirmDialog = false;
  }
},

  closeSuccessDialog() {
    this.showSuccessDialog = false;
    this.$emit('cancel-success');
    this.$emit('close');
  },
    async submitCancellation() {
  if (!this.cancelReason.trim()) {
    alert('Please enter a cancellation message.');
    return;
  }
  this.showConfirmDialog = true; 
}
  }
};</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.55);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999;
    }

    .modal-content {
        background: #ffffff;
        border-radius: 16px;
        padding: 32px;
        width: 720px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 12px 30px rgba(0, 0, 0, 0.15);
        font-family: 'Segoe UI', sans-serif;
    }

    .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 28px;
    }

        .modal-header h2 {
            font-size: 24px;
            font-weight: 700;
            color: #333;
        }

    .close-btn {
        font-size: 26px;
        border: none;
        background: none;
        cursor: pointer;
        color: #999;
    }

    .course-summary {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 10px 20px;
        font-size: 14.5px;
        line-height: 1.4;
        padding-bottom: 24px;
        border-bottom: 1px solid #e0e0e0;
        color: #333;
    }

        .course-summary div strong {
            color: #000;
        }

    .cancel-reason-box {
        margin-top: 24px;
    }

    .email-label {
        font-weight: 600;
        font-size: 15px;
        margin-bottom: 6px;
        display: inline-block;
        color: #444;
    }

    .quill-box {
        background: #fff;
        border: 1px solid #ccc;
        border-radius: 10px;
        min-height: 280px;
        margin-top: 10px;
        font-family: 'Segoe UI', sans-serif;
    }

        .quill-box .ql-editor {
            font-size: 15px;
            padding: 14px;
            line-height: 1.5;
            color: #333;
            max-height: 350px;
            overflow-y: auto;
        }

            .quill-box .ql-editor p {
                margin: 3px 0;
            }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 24px;
    }

    .btn-primary {
        background-color: #c62828;
        color: white;
        font-weight: 600;
        padding: 10px 20px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-primary:hover {
            background-color: #a42424;
        }

    .btn-secondary {
        background-color: #f1f1f1;
        color: #333;
        padding: 10px 20px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-secondary:hover {
            background-color: #ddd;
        }
/*    .modal.confirmation {
        border-top: 6px solid #ffc107;
        animation: fadeInScale 0.3s ease;
    }*/
    .modal.confirmation,
    .modal.success {
        z-index: 10000;
    }
        .modal.confirmation h3 {
            color: #ff9800;
            font-weight: bold;
            text-align: center;
        }

    .modal.success {
        border-top: 6px solid #4CAF50;
        animation: fadeInScale 0.3s ease;
    }

        .modal.success h3 {
            color: #388e3c;
            font-weight: bold;
            text-align: center;
        }

        .modal.success p,
        .modal.confirmation p {
            text-align: center;
            color: #444;
        }
    /* Reuse modal styling for both confirmation and success dialogs */
    .modal.confirmation,
    .modal.success {
        background: #ffffff;
        border-radius: 16px;
        padding: 32px;
        width: 480px;
        max-width: 90vw;
        box-shadow: 0 16px 40px rgba(0, 0, 0, 0.25);
        font-family: 'Segoe UI', sans-serif;
        text-align: center;
        animation: fadeInScale 0.3s ease;
        z-index: 10001;
    }

    /* Confirmation Modal */
    .modal.confirmation {
        border-top: 6px solid #ffc107;
    }

        .modal.confirmation h3 {
            color: #ff9800;
            font-size: 22px;
            margin-bottom: 10px;
            font-weight: 700;
        }

        .modal.confirmation p {
            font-size: 16px;
            margin: 16px 0 24px;
            color: #444;
        }

    /* Success Modal */
    .modal.success {
        border-top: 6px solid #4CAF50;
    }

        .modal.success h3 {
            color: #388e3c;
            font-size: 22px;
            margin-bottom: 10px;
            font-weight: 700;
        }

        .modal.success p {
            font-size: 16px;
            margin: 16px 0 24px;
            color: #444;
        }

    /* Shared button group styling */
    .modal .button-group {
        display: flex;
        justify-content: center;
        gap: 16px;
        margin-top: 10px;
    }

    .btn-danger {
        background-color: #e53935;
        color: white;
        padding: 10px 20px;
        border: none;
        font-weight: 600;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-danger:hover {
            background-color: #c62828;
        }

    /* Reuse existing secondary/primary buttons */
    .btn-primary {
        background-color: #1976d2;
        color: white;
        padding: 10px 20px;
        border-radius: 8px;
        font-weight: 600;
        border: none;
        cursor: pointer;
    }

        .btn-primary:hover {
            background-color: #1565c0;
        }

    .btn-secondary {
        background-color: #eeeeee;
        color: #333;
        padding: 10px 20px;
        border-radius: 8px;
        border: none;
        font-weight: 500;
        cursor: pointer;
    }

        .btn-secondary:hover {
            background-color: #d6d6d6;
        }

    /* Subtle animation for modal popup */
    @keyframes fadeInScale {
        from {
            opacity: 0;
            transform: scale(0.95);
        }

        to {
            opacity: 1;
            transform: scale(1);
        }
    }
</style>