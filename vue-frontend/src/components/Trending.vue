<template>
    <div class="trending-container">
        <h1>Trending</h1>
        <p>
            Explore the latest resources and tips for enhancing remote collaboration and client engagement.
        </p>
        <ul>
            <li>
                <a href="#" target="_blank">Meeting and Working with Clients Remotely Resources</a>
            </li>
            <li>
                <a href="#" target="_blank">Comparison of Video Conferencing Platforms that Clients Commonly Use</a>
            </li>
            <li>
                <a href="#" target="_blank">How to Make Your Virtual Staff Meetings More Interactive</a>
            </li>
            <li>
                <a href="#" target="_blank">Preparing to Meet with Clients Remotely: A Checklist</a>
            </li>
            <li>
                <a href="#" target="_blank">Tips to Meet with Providers with Confidence</a>
            </li>
            <li>
                <a href="#" target="_blank">Tutorials for Video Conferencing Platforms</a>
            </li>
        </ul>
    </div>
</template>

<script>export default {
        name: "TrendingPage",
    };</script>

<style scoped>
    .trending-container {
        padding: 30px;
        max-width: 900px;
        margin: 0 auto;
        background-color: #f9f9f9;
        border-radius: 8px;
        box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
        font-family: Arial, sans-serif;
        color: #333;
    }

    h1 {
        font-size: 2rem;
        color: #3f51b5;
        margin-bottom: 16px;
    }

    p {
        font-size: 1.1rem;
        line-height: 1.6;
        margin-bottom: 16px;
    }

    ul {
        list-style: disc;
        margin: 16px 0;
        padding-left: 20px;
    }

        ul li {
            margin-bottom: 8px;
        }

    a {
        color: #3f51b5;
        text-decoration: none;
        font-weight: bold;
    }

        a:hover {
            text-decoration: underline;
        }
</style>
