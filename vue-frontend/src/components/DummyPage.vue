<template>
    <div class="home-page-container">
        <!-- ✅ Ensure SideNav is only rendered once -->
        <template v-if="!isSideNavRendered">
            <SideNav :isUserLoggedIn="isUserLoggedIn" />
        </template>

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
    </div>
</template>

<script>
    import eventBus from "@/eventBus.js"; // ✅ Import eventBus
    import LoginComponent from "@/components/LoginComponent.vue";
    import RegistrationModal from "@/components/RegistrationModal.vue";
    import SideNav from "@/components/SideNav.vue";

    export default {
        name: "HomePage",
        components: {
            LoginComponent,
            RegistrationModal,
            SideNav,
        },
        data() {
            return {
                userName: localStorage.getItem("userName") || null,
                isUserLoggedIn: !!localStorage.getItem("jwtToken"),
                showLoginModal: false,
                showRegisterModal: false,
                isSideNavRendered: false, // ✅ Ensure SideNav only renders once
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
            this.isSideNavRendered = true;
        },
        beforeUnmount() {
            clearInterval(this.carouselInterval);
        },
        methods: {
            startImageCarousel() {
                this.carouselInterval = setInterval(() => {
                    this.currentImageIndex = (this.currentImageIndex + 1) % this.images.length;
                }, 3000);
            },
            async handleLoginSuccess(userData) {
                console.log("🔥 Full Login Response:", userData); // ✅ Log full response

                if (!userData || !userData.userId) {
                    alert("⚠️ Login successful, but user data is missing.");
                    console.error("🚨 UserId is missing in response:", userData);
                    return;
                }

                // ✅ Store Correct Data in localStorage
                localStorage.setItem("userId", userData.userId);  // 🔥 Use userId instead of userSysID
                localStorage.setItem("userName", userData.firstName + " " + userData.lastName);
                localStorage.setItem("jwtToken", userData.token);

                console.log("✅ Stored user details:", {
                    userId: localStorage.getItem("userId"),
                    userName: localStorage.getItem("userName"),
                    jwtToken: localStorage.getItem("jwtToken"),
                });

                eventBus.emit("auth-change"); // ✅ Notify other components
                this.reloadPage();
            },
            handleLogout() {
                localStorage.removeItem("jwtToken");
                localStorage.removeItem("userName");
                this.reloadPage(); // ✅ Reload page to update UI
            },
            handleShowRegister() {
                this.showRegistrationForm = true;
                this.showLoginModal = false;
            },
            reloadPage() {
                setTimeout(() => {
                    window.location.reload(); // 🔄 Full page reload
                }, 500);
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