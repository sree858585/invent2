<template>
    <div class="scorm-container">
        <header class="scorm-toolbar">
            <button class="back-btn" @click="$emit('exit')">← Back</button>
            <div class="title">{{ title || 'Course Player' }}</div>
            <div class="spacer"></div>
        </header>

        <div class="scorm-stage">
            <iframe ref="iframeRef"
                    :src="launchUrl"
                    class="scorm-iframe"
                    allow="fullscreen"
                    title="SCORM Content" />
        </div>
    </div>
</template>

<script setup>/* global defineProps */
import { onMounted, onBeforeUnmount, ref } from 'vue'
import apiClient from '@/axios'

// Props: launchUrl (same-origin), registrationId, scoId, preloadCmi (object), title
// eslint-disable-next-line no-undef
const props = defineProps({
  launchUrl: { type: String, required: true },
  registrationId: { type: String, required: true },
  scoId: { type: String, required: true },
  preloadCmi: { type: Object, default: () => ({}) },
  title: { type: String, default: '' }
})

const iframeRef = ref(null)
let api = null
let commitTimer = null

function createScorm12Api({ registrationId, scoId, preload }) {
  let initialized = false
  let lastError = '0'
  const cmi = new Map(Object.entries(preload || {}))
  const pending = new Map()

  const setPending = (el, val) => {
    pending.set(el, String(val ?? ''))
    cmi.set(el, String(val ?? ''))
  }

  async function commitNow() {
    if (pending.size === 0) return 'true'
    const data = Array.from(pending.entries()).map(([element, value]) => ({ element, value }))
    pending.clear()
    try {
      await apiClient.post(`/scorm/runtime/${registrationId}/commit`, { scoId, data })
      return 'true'
    } catch (e) {
      lastError = '391'
      data.forEach(({ element, value }) => pending.set(element, value))
      return 'false'
    }
  }

  const sessionStart = Date.now()
  function getSessionTime() {
    const ms = Date.now() - sessionStart
    const totalSec = Math.floor(ms / 1000)
    const hh = String(Math.floor(totalSec / 3600)).padStart(4, '0')
    const mm = String(Math.floor((totalSec % 3600) / 60)).padStart(2, '0')
    const ss = String(totalSec % 60).padStart(2, '0')
    return `${hh}:${mm}:${ss}.00`
  }

  const API = {
    // Removed unused params
    LMSInitialize: () => {
      if (initialized) { lastError = '101'; return 'false' }
      initialized = true
      lastError = '0'
      return 'true'
    },
    LMSFinish: async () => {
      await commitNow()
      try {
        await apiClient.post(`/scorm/runtime/${registrationId}/finish`, {
          scoId,
          session_time: getSessionTime()
        })
        initialized = false
        lastError = '0'
        return 'true'
      } catch {
        lastError = '391'
        return 'false'
      }
    },
    LMSGetValue: (element) => {
      if (!initialized) { lastError = '301'; return '' }
      lastError = '0'
      return cmi.get(element) ?? ''
    },
    LMSSetValue: (element, value) => {
      if (!initialized) { lastError = '301'; return 'false' }
      if (element === 'cmi.suspend_data' && String(value).length > 4096) {
        lastError = '405'
        return 'false'
      }
      setPending(element, value)
      if (element === 'cmi.core.lesson_status') {
        const v = String(value).toLowerCase()
        if (!['passed','failed','completed','incomplete','browsed','not attempted'].includes(v)) {
          lastError = '405'; return 'false'
        }
      }
      lastError = '0'
      return 'true'
    },
    LMSCommit: async () => {
      return await commitNow()
    },
    LMSGetLastError: () => lastError,
    LMSGetErrorString: (code) => code,
    LMSGetDiagnostic: (code) => code
  }

  return { API, commitNow }
}

onMounted(() => {
  const { API: scormApi, commitNow } = createScorm12Api({
    registrationId: props.registrationId,
    scoId: props.scoId,
    preload: props.preloadCmi
  })
  api = { scormApi, commitNow }

  // Expose SCORM API to child
  window.API = scormApi

  // Auto-commit
  commitTimer = setInterval(() => api?.commitNow(), 60_000)

  const beforeUnload = () => api?.commitNow()
  window.addEventListener('beforeunload', beforeUnload)

  onBeforeUnmount(() => {
    window.removeEventListener('beforeunload', beforeUnload)
  })
})

onBeforeUnmount(() => {
  if (commitTimer) clearInterval(commitTimer)
  if (window.API) delete window.API
})</script>

<style scoped>
    .scorm-container {
        display: flex;
        flex-direction: column;
        height: 100%;
        background: #fff;
        border-radius: 16px;
        box-shadow: 0 6px 20px rgba(0,0,0,.08);
        overflow: hidden;
    }

    /* Sticky toolbar with back button */
    .scorm-toolbar {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 10px 14px;
        border-bottom: 1px solid #eee;
        background: #f9fafb;
    }

    .back-btn {
        border: none;
        background: #e5e7eb;
        padding: 8px 12px;
        border-radius: 10px;
        font-weight: 600;
        cursor: pointer;
    }

    .title {
        font-weight: 700;
    }

    .spacer {
        flex: 1;
    }

    /* Stage keeps a good aspect on desktop and fills on mobile */
    .scorm-stage {
        flex: 1;
        min-height: 60vh;
        max-height: calc(100vh - 60px);
    }

    .scorm-iframe {
        width: 100%;
        height: calc(100vh - 56px); /* fills below toolbar */
        border: 0;
    }

    /* Mobile tweaks */
    @media (max-width: 768px) {
        .scorm-iframe {
            height: calc(100vh - 56px);
        }
    }
</style>