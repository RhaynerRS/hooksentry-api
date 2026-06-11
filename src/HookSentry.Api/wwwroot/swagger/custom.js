/**
 * HookSentry — Swagger UI Customization
 *
 * Expõe SwaggerCustomizer com métodos para alterar favicon e logo.
 * É executado automaticamente ao carregar a página.
 *
 * Para ativar logo/favicon, preencha CONFIG abaixo com as URLs reais.
 * Enquanto estiverem vazios (''), nada é alterado e nenhum erro ocorre.
 */
(function () {
    'use strict';

    const SwaggerCustomizer = {
        /**
         * Substitui o favicon da página.
         * @param {string} url   URL ou data-URI da imagem do favicon.
         * @param {string} [type='image/png']  MIME type do favicon.
         */
        setFavicon(url, type = 'image/png') {
            if (!url) return;

            ['link[rel="icon"]', 'link[rel="shortcut icon"]'].forEach(sel => {
                const el = document.querySelector(sel);
                if (el) el.remove();
            });

            const link = document.createElement('link');
            link.rel  = 'icon';
            link.type = type;
            link.href = url;
            document.head.appendChild(link);
        },

        /**
         * Substitui o logo exibido na top-bar do Swagger.
         *
         * Se a imagem falhar ao carregar (404, etc.), remove o elemento <img>
         * para evitar o ícone de imagem quebrada.
         *
         * @param {string} url          URL ou data-URI do logo.
         * @param {string} [alt]        Texto alternativo.
         * @param {number} [height=32]  Altura em px.
         */
        setLogo(url, alt = 'HookSentry', height = 32) {
            if (!url) return;

            const inject = () => {
                const wrapper = document.querySelector('.topbar-wrapper .link')
                    || document.querySelector('.topbar-wrapper a')
                    || document.querySelector('.topbar-wrapper');

                if (!wrapper) return false;

                // Remove SVG e imgs existentes (exceto o nosso)
                wrapper.querySelector('svg')?.remove();
                wrapper.querySelector('img:not(#hs-custom-logo)')?.remove();

                // Remove injeção anterior para evitar duplicata
                document.getElementById('hs-custom-logo')?.remove();

                const img = document.createElement('img');
                img.id             = 'hs-custom-logo';
                img.src            = url;
                img.alt            = alt;
                img.style.cssText  = `height:${height}px;width:auto;object-fit:contain;`;

                // Se a imagem não carregar, remove o ícone quebrado
                img.onerror = () => img.remove();

                wrapper.insertBefore(img, wrapper.firstChild);
                return true;
            };

            if (!inject()) {
                const obs = new MutationObserver(() => {
                    if (inject()) obs.disconnect();
                });
                obs.observe(document.body, { childList: true, subtree: true });
            }
        },

        /**
         * Aplica favicon e logo de uma só vez.
         * @param {object} opts
         * @param {string} [opts.faviconUrl]
         * @param {string} [opts.faviconType]
         * @param {string} [opts.logoUrl]
         * @param {string} [opts.logoAlt]
         * @param {number} [opts.logoHeight]
         */
        apply({ faviconUrl, faviconType, logoUrl, logoAlt, logoHeight } = {}) {
            if (faviconUrl) this.setFavicon(faviconUrl, faviconType);
            if (logoUrl)    this.setLogo(logoUrl, logoAlt, logoHeight);
        },
    };

    window.SwaggerCustomizer = SwaggerCustomizer;

    // ----------------------------------------------------------------
    // CONFIG — preencha as URLs reais para ativar logo e favicon.
    // Strings vazias ('') desativam o respectivo elemento sem erros.
    // ----------------------------------------------------------------
    const CONFIG = {
        faviconUrl:  '',          // ex: '/swagger/favicon.png'
        faviconType: 'image/png',
        logoUrl:     '',          // ex: '/swagger/logo.png'
        logoAlt:     'HookSentry',
        logoHeight:  32,
    };

    function autoApply() {
        SwaggerCustomizer.apply(CONFIG);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', autoApply);
    } else {
        autoApply();
    }
})();
