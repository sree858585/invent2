<template>
  <div class="course-module-container">
    <h1>HIV Training Course</h1>

    <div class="content-wrapper">
      <!-- Main Content -->
      <div class="main-content" v-if="currentChapter">
        <!-- Chapter Title -->
        <div class="content-section">
          <h2>{{ currentChapter.title }}</h2>
          <p>{{ currentChapter.description }}</p>
        </div>

        <!-- Video Section -->
        <div
          class="video-container"
          :class="{ enlarged: activeVideo === currentChapter.id, completed: completedVideos[currentChapter.id] }"
          @click="toggleVideoExpansion(currentChapter.id)"
        >
          <h3>{{ currentChapter.videoTitle }}</h3>
          <video
            :src="currentChapter.videoSrc"
            controls
            @ended="markVideoCompleted(currentChapter.id)"
          ></video>
          <div v-if="completedVideos[currentChapter.id]" class="completion-check">✔</div>
        </div>

        <!-- PDF Viewer -->
        <div class="pdf-viewer-container" v-if="currentChapter.pdfSrc">
          <h3>{{ currentChapter.pdfTitle }}</h3>
          <iframe
            :src="currentChapter.pdfSrc"
            width="100%"
            height="500px"
            style="border: 1px solid #ccc; border-radius: 8px;"
          ></iframe>
        </div>

        <!-- Quiz Section -->
        <div class="quiz-section" v-if="currentChapter.isQuiz">
          <h2>Quiz Assessment</h2>
          <div v-for="(question, i) in currentChapter.quiz" :key="i" class="quiz-question" v-show="currentQuestionIndex === i">
            <h3>Question {{ i + 1 }}</h3>
            <p>{{ question.text }}</p>
            <div class="options-container">
              <label v-for="option in question.options" :key="option" :class="{ selected: selectedAnswers[i] === option }" class="option">
                <input type="radio" :name="'question-' + i" :value="option" @change="selectAnswer(i, option)" />
                {{ option }}
              </label>
            </div>
          </div>

          <div class="quiz-navigation">
            <button @click="previousQuestion" :disabled="currentQuestionIndex === 0">Previous</button>
            <button v-if="currentQuestionIndex < currentChapter.quiz.length - 1" @click="nextQuestion">Next</button>
            <button v-else @click="submitQuiz">Submit</button>
          </div>
        </div>
      </div>

      <!-- Right Navigation -->
      <div class="right-nav">
        <ul>
          <li
            v-for="chapter in chapters"
            :key="chapter.id"
            :class="{ active: currentChapter?.id === chapter.id }"
            @click="selectChapter(chapter.id)"
          >
            {{ chapter.title }}
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: "CourseModule",
  data() {
    return {
      currentChapter: null,
      activeVideo: null,
      completedVideos: {}, // Track completed videos
      currentQuestionIndex: 0, // Track quiz question index
      selectedAnswers: {}, // Store user answers
      chapters: [
        {
          id: 1,
          title: "Chapter 1: Introduction to HIV",
          description: "Introduction to the basics of HIV.",
          videoTitle: "Introduction to HIV",
          videoSrc: "/videos/introduction.mp4", // Video URL added
          pdfTitle: "Basics of HIV",
          pdfSrc: "/pdfs/dummy.pdf",
          isQuiz: false,
        },
        {
          id: 2,
          title: "Chapter 2: Prevention and Care",
          description: "Learn about prevention methods and care for HIV.",
          videoTitle: "Prevention and Care",
          videoSrc: "/videos/introduction.mp4",
          pdfTitle: "HIV Prevention Guidelines",
          pdfSrc: "/pdfs/dummy.pdf",
          isQuiz: false,
        },
        {
          id: 3,
          title: "Quiz Assessment",
          description: "Test your knowledge with a quiz.",
          isQuiz: true,
          quiz: [
            {
              text: "What does HIV stand for?",
              options: [
                "Human Immune Virus",
                "Human Immunodeficiency Virus",
                "Human Inherited Virus",
                "Human Infection Virus",
              ],
            },
            {
              text: "What is a common method of HIV prevention?",
              options: [
                "Using PrEP",
                "Ignoring symptoms",
                "Sharing needles",
                "Not using condoms",
              ],
            },
          ],
        },
      ],
    };
  },
  created() {
    this.selectChapter(1); // Load the first chapter by default
  },
  methods: {
    selectChapter(chapterId) {
      this.currentChapter = this.chapters.find((chapter) => chapter.id === chapterId);
      this.currentQuestionIndex = 0; // Reset quiz index when chapter changes
    },
    toggleVideoExpansion(videoId) {
      this.activeVideo = this.activeVideo === videoId ? null : videoId; // Toggle video view
    },
    markVideoCompleted(videoId) {
      this.completedVideos[videoId] = true; // Mark video as completed
    },
    selectAnswer(index, option) {
      this.selectedAnswers[index] = option; // Store selected quiz answer
    },
    previousQuestion() {
      if (this.currentQuestionIndex > 0) this.currentQuestionIndex--;
    },
    nextQuestion() {
      if (this.currentQuestionIndex < this.currentChapter.quiz.length - 1) this.currentQuestionIndex++;
    },
    submitQuiz() {
      console.log("Quiz submitted. Answers:", this.selectedAnswers);
      alert("Quiz submitted. Check console for answers.");
    },
  },
};
</script>


<style scoped>
.course-module-container {
  display: flex;
  flex-direction: column;
  padding: 20px;
}

h1 {
  text-align: center;
  color: #1e88e5;
  margin-bottom: 24px;
}

.content-wrapper {
  display: flex;
  gap: 20px;
}

.main-content {
  flex: 1;
  padding: 20px;
  background-color: white;
  border-radius: 8px;
}

.right-nav {
  width: 220px;
}

.right-nav ul {
  list-style: none;
  padding: 0;
}

.right-nav li {
  cursor: pointer;
  padding: 10px;
  margin-bottom: 10px;
  background-color: #e3f2fd;
  border: 1px solid #1e88e5;
  border-radius: 8px;
}

.right-nav li.active {
  font-weight: bold;
  background-color: #bbdefb;
}

.video-container {
  position: relative;
}

.completion-check {
  position: absolute;
  top: 10px;
  right: 10px;
  background: green;
  color: white;
  border-radius: 50%;
  padding: 5px 10px;
}

.options-container {
  display: flex;
  flex-direction: column;
}
</style>
