window.sr = {
  aplicarTema: function (tema) {
    document.documentElement.setAttribute("data-theme", tema);
  },
  arriba: function () { window.scrollTo(0, 0); }
};
