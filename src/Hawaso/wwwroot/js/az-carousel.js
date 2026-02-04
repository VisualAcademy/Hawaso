/**
 * Az Carousel (Improved)
 * - 컨테이너(섹션)별로 독립 동작 (여러 개 있어도 OK)
 * - requestAnimationFrame 중복/누적 방지 (탭 전환 시 프로그레스바 동시 진행 버그 해결)
 * - 탭 비가시 상태(document.hidden)면 자동 pause, 다시 보이면 자동 resume
 */

(function () {
    const SLIDE_DURATION = 5000;

    const sections = document.querySelectorAll('.az-carousel-section');
    if (!sections || sections.length === 0) return;

    const instances = [];

    sections.forEach(section => {
        const slides = section.querySelectorAll('.az-slide');
        const totalSlides = slides.length;

        const progressIndicators = section.querySelector('.az-progress-indicators');
        const nextBtn = section.querySelector('.az-next-btn');
        const prevBtn = section.querySelector('.az-prev-btn');

        if (!progressIndicators || !nextBtn || !prevBtn || totalSlides === 0) return;

        // 혹시 서버 렌더링/재사용 상황에서 기존 내용이 있을 수 있으니 초기화
        progressIndicators.innerHTML = '';

        const state = {
            section,
            slides,
            totalSlides,
            progressIndicators,
            nextBtn,
            prevBtn,

            currentIndex: 0,
            isPaused: false,
            autoPausedByVisibility: false,

            timeoutId: null,
            rafId: null,

            startTime: 0,
            elapsed: 0,
            slideDuration: SLIDE_DURATION,

            progressFills: [],
            pauseBtn: null
        };

        // Progress dots
        for (let i = 0; i < totalSlides; i++) {
            const dot = document.createElement('div');
            dot.className = 'az-progress-dot';
            dot.dataset.index = i;

            const fill = document.createElement('div');
            fill.className = 'az-progress-fill';
            dot.appendChild(fill);

            dot.addEventListener('click', () => {
                clearTimeout(state.timeoutId);
                cancelRaf(state);
                state.elapsed = 0;
                updateSlide(state, i);
            });

            progressIndicators.appendChild(dot);
            state.progressFills.push(fill);
        }

        // Pause button
        const pauseBtn = document.createElement('div');
        pauseBtn.className = 'az-pause-button';
        pauseBtn.innerHTML = '<i class="fas fa-pause"></i>';
        progressIndicators.appendChild(pauseBtn);
        state.pauseBtn = pauseBtn;

        pauseBtn.addEventListener('click', () => {
            state.isPaused = !state.isPaused;
            pauseBtn.innerHTML = `<i class="fas fa-${state.isPaused ? 'play' : 'pause'}"></i>`;

            if (state.isPaused) {
                pauseProgress(state);
            } else {
                resumeProgress(state);
            }
        });

        // Next / Prev
        nextBtn.addEventListener('click', () => {
            clearTimeout(state.timeoutId);
            cancelRaf(state);
            state.elapsed = 0;
            updateSlide(state, state.currentIndex + 1);
        });

        prevBtn.addEventListener('click', () => {
            clearTimeout(state.timeoutId);
            cancelRaf(state);
            state.elapsed = 0;
            updateSlide(state, state.currentIndex - 1);
        });

        // Touch swipe
        let touchStartX = 0;
        let touchEndX = 0;

        section.addEventListener('touchstart', e => {
            touchStartX = e.changedTouches[0].screenX;
        });

        section.addEventListener('touchend', e => {
            touchEndX = e.changedTouches[0].screenX;
            handleSwipe(state, touchStartX, touchEndX);
        });

        // Start
        updateSlide(state, 0);
        instances.push(state);
    });

    // Visibility change: hidden -> auto pause, visible -> auto resume
    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            instances.forEach(state => {
                if (!state.isPaused) {
                    state.autoPausedByVisibility = true;
                    pauseProgress(state);
                }
            });
        } else {
            instances.forEach(state => {
                if (state.autoPausedByVisibility) {
                    state.autoPausedByVisibility = false;
                    if (!state.isPaused) resumeProgress(state);
                }
            });
        }
    });

    // ---- helpers ----

    function cancelRaf(state) {
        if (state.rafId) {
            cancelAnimationFrame(state.rafId);
            state.rafId = null;
        }
    }

    function updateSlide(state, index) {
        state.currentIndex = (index + state.totalSlides) % state.totalSlides;

        state.slides.forEach((slide, i) => {
            slide.classList.toggle('az-active', i === state.currentIndex);
        });

        // reset all progress fills
        state.progressFills.forEach(fill => {
            fill.style.transition = 'none';
            fill.style.width = '0%';
        });

        clearTimeout(state.timeoutId);
        cancelRaf(state);

        state.elapsed = 0;

        if (!state.isPaused && !document.hidden) {
            startProgress(state, state.slideDuration);
        }
    }

    function startProgress(state, duration) {
        const fill = state.progressFills[state.currentIndex];
        if (!fill) return;

        fill.style.transition = 'none';
        fill.style.width = '0%';

        // force reflow so the "transition reset" is applied
        void fill.offsetWidth;

        cancelRaf(state);
        state.rafId = requestAnimationFrame(() => {
            fill.style.transition = `width ${duration}ms linear`;
            fill.style.width = '100%';
        });

        state.startTime = Date.now();
        state.timeoutId = setTimeout(() => {
            state.elapsed = 0;
            updateSlide(state, state.currentIndex + 1);
        }, duration);
    }

    function pauseProgress(state) {
        const fill = state.progressFills[state.currentIndex];
        if (!fill) return;

        const computedWidth = parseFloat(getComputedStyle(fill).width) || 0;
        const parentWidth = fill.parentElement ? fill.parentElement.offsetWidth : 0;
        const percent = parentWidth > 0 ? (computedWidth / parentWidth) * 100 : 0;

        fill.style.transition = 'none';
        fill.style.width = Math.max(0, Math.min(100, percent)) + '%';

        if (state.startTime) {
            state.elapsed += Date.now() - state.startTime;
        }

        clearTimeout(state.timeoutId);
        cancelRaf(state);
    }

    function resumeProgress(state) {
        if (document.hidden) return;

        const remaining = Math.max(0, state.slideDuration - state.elapsed);
        startProgress(state, remaining > 0 ? remaining : state.slideDuration);
    }

    function handleSwipe(state, startX, endX) {
        if (endX < startX - 30) {
            clearTimeout(state.timeoutId);
            cancelRaf(state);
            state.elapsed = 0;
            updateSlide(state, state.currentIndex + 1);
        } else if (endX > startX + 30) {
            clearTimeout(state.timeoutId);
            cancelRaf(state);
            state.elapsed = 0;
            updateSlide(state, state.currentIndex - 1);
        }
    }
})();
