<template>
    <div class="home-page-container">
        <!-- ✅ Pass isUserLoggedIn as a prop -->
        <SideNav :isUserLoggedIn="isUserLoggedIn" />
        <!-- Header Section -->

        <div class="header-section">
            <h1 v-if="!userName">Welcome to the Online Registration System for NYS AIDS Institute Training Centers</h1>
            <h1 v-else>Hello, {{ userName }}</h1>
            <p><strong>Please Note:</strong> The preferred web browser for using this site is Google Chrome.</p>
        </div>

        <!-- Animated Image Carousel -->
        <div class="image-carousel">
            <img :src="images[currentImageIndex].src"
                 :alt="images[currentImageIndex].alt"
                 class="carousel-image" />
        </div>

        <!-- Login / Logout Buttons -->
        <button v-if="!userName" class="register-btn" @click="showLoginModal = true">Login</button>
        <button v-if="userName" class="register-btn logout-btn" @click="handleLogout">Logout</button>

        <!-- Modals -->
        <LoginComponent v-if="showLoginModal"
                        @login-success="handleLoginSuccess"
                        @show-register="handleShowRegister"
                        @close="showLoginModal = false" />

        <RegistrationModal v-if="showRegistrationForm"
                           @close="showRegistrationForm = false" />

        <!-- Important Information Section -->
        <div class="important-info-section">
            <h2>Important Course Registration Information:</h2>
            <p>
                Once you have registered for a course, you will receive a confirmation email with details about the course.
                This email can sometimes end up in a junk or spam folder, so please check those places if you do not see it in
                your inbox. You can also find all of your course information by going to ‘My Courses’ on the right-hand column
                of this page. You must be logged into your account to access the ‘My Courses’ page.
            </p>
        </div>
        <p>
            We offer HIV, sexually transmitted infection (STI), and viral hepatitis trainings across New York State.
        </p>
        <p>
            The trainings on this site are intended for non-physician health and human services providers who offer HIV,
            STI, and viral hepatitis prevention, testing, care, and support services. All trainings are free of charge and
            funded by the New York State Department of Health AIDS Institute.
        </p>
        <p>
            For clinical trainings, please visit <a href="https://ceitraining.org" target="_blank">ceitraining.org</a>.
        </p>
        <p>
            If you require assistance with registering for a course, please contact the training center listed on the
            course description. For a list of training centers and their contact information, click
            <a href="#">here</a>.
        </p>
    </div>

    <!-- Explore Our Training Section -->
    <div class="explore-training-section">
        <h2>Explore Our Training Programs</h2>
        <div class="training-cards">
            <div class="card">
                <img src="/images/in-person-training.jpg" alt="In-Person Training" />
                <h3>In-Person Training</h3>
                <p>Join face-to-face sessions for an immersive learning experience.</p>
            </div>
            <div class="card">
                <img src="/images/online-training.jpg" alt="Online Training" />
                <h3>Online Training</h3>
                <p>Access our extensive library of online courses from anywhere.</p>
            </div>
            <div class="card">
                <img src="/images/hybrid-training.jpg" alt="Hybrid Training" />
                <h3>Hybrid Training</h3>
                <p>Combine in-person and online learning for maximum flexibility.</p>
            </div>
        </div>
    </div>

    <!-- Register Modal -->
    <div v-if="showRegisterModal" class="modal-overlay">
        <div class="modal">
            <h3>Register for a Course</h3>
            <form>
                <div class="form-group">
                    <label for="email">Email Address</label>
                    <input type="email" id="email" placeholder="Enter your email" />
                </div>
                <div class="form-group">
                    <label for="password">Password</label>
                    <input type="password" id="password" placeholder="Enter your password" />
                </div>
                <button type="submit" class="btn-primary">Login</button>
            </form>
            <p>
                <a href="#" @click.prevent="showRegistrationForm = true; showRegisterModal = false">Register if you don't have an account.</a>
            </p>
            <button class="close-btn" @click="showRegisterModal = false">Close</button>
        </div>
    </div>
    <!-- Registration Form Modal -->
    <RegistrationModal v-if="showRegistrationForm"
                       @close="showRegistrationForm = false" />
</template>

<script>import LoginComponent from "@/components/LoginComponent.vue";
    import RegistrationModal from "@/components/RegistrationModal.vue";

    export default {
        name: "HomePage",
        components: {
            LoginComponent,
            RegistrationModal // Registering the component
        },
        data() {
            return {
                userName: localStorage.getItem("userName") || null,
                isUserLoggedIn: !!localStorage.getItem("jwtToken"), // ✅ Reactive Login State
                showLoginModal: false,
                showRegisterModal: false,
                currentImageIndex: 0,
                images: [
                    { src: "/images/img1.jpeg", alt: "Image 1" },
                    { src: "/images/img2.jpeg", alt: "Image 2" },
                    { src: "/images/img3.jpeg", alt: "Image 3" },
                    { src: "/images/img4.jpeg", alt: "Image 4" },
                    { src: "/images/img5.jpeg", alt: "Image 5" },
                ],
            };
        },
        mounted() {
            this.startImageCarousel();
        },
        beforeUnmount() {
            clearInterval(this.carouselInterval);
        },
        methods: {
            startImageCarousel() {
                this.carouselInterval = setInterval(() => {
                    this.currentImageIndex =
                        (this.currentImageIndex + 1) % this.images.length;
                }, 3000);
            },
            handleLoginSuccess(userName) {
                this.userName = userName;
                localStorage.setItem("userName", userName);
            },
            handleLogout() {
                localStorage.removeItem("jwtToken");
                localStorage.removeItem("userName");
                this.userName = null;
            },
            handleShowRegister() {
                this.showRegistrationForm = true;
                this.showLoginModal = false;  // ✅ Close login modal when opening registration
            }
        },
    };</script>


<style scoped>
    /* General Container */
    .home-page-container {
        font-family: Arial, sans-serif;
        line-height: 1.6;
        color: #333;
        padding: 20px;
        position: relative;
    }

    /* Header Section */
    .header-section {
        text-align: center;
        margin-bottom: 20px;
    }

        .header-section h1 {
            font-size: 1.8rem;
            color: #1e88e5;
        }

    /* Animated Image Carousel */
    .image-carousel {
        width: 300px; /* Set width of the square */
        height: 300px; /* Equal height to make it a square */
        margin: 20px auto; /* Center the container */
        overflow: hidden; /* Hide overflow to ensure square shape */
        border-radius: 8px; /* Optional: Rounded corners */
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2); /* Add a shadow for aesthetics */
        display: flex;
        justify-content: center;
        align-items: center;
        background-color: #f9f9f9; /* Light background */
    }

    .carousel-image {
        width: 100%;
        height: 100%;
        object-fit: cover; /* Ensure the image fills the container while maintaining aspect ratio */
    }

    /* Register Button */
    .register-btn {
        position: absolute;
        top: 20px;
        right: 20px;
        background-color: #3f51b5;
        color: white;
        border: none;
        padding: 8px 12px;
        border-radius: 4px;
        cursor: pointer;
        transition: background-color 0.3s;
    }

        .register-btn:hover {
            background-color: #303f9f;
        }

    /* Modal Overlay */
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    /* Modal */
    .modal {
        background-color: white;
        padding: 20px;
        border-radius: 8px;
        width: 300px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        text-align: center;
    }

        .modal h3 {
            margin-bottom: 20px;
            color: #3f51b5;
        }

        .modal .form-group {
            margin-bottom: 15px;
        }

        .modal input {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .modal .btn-primary {
            background-color: #3f51b5;
            color: white;
            padding: 8px 12px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            width: 100%;
        }

            .modal .btn-primary:hover {
                background-color: #303f9f;
            }

        .modal .close-btn {
            margin-top: 10px;
            background: none;
            color: #3f51b5;
            border: none;
            cursor: pointer;
            font-size: 14px;
            text-decoration: underline;
        }

            .modal .close-btn:hover {
                color: #1e88e5;
            }
</style>