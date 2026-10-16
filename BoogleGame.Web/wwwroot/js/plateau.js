// Plateau de jeu : affichage des dés, sélection à la souris / au doigt / au clavier, tracé du chemin

const AMBIGUES = new Set(['M', 'W', 'N', 'Z']); // soulignées, comme sur les vrais dés

export class Plateau {
  constructor(conteneur, { taille, cases, interactif = true, onMot = () => {}, onChange = () => {} }) {
    this.taille = taille;
    this.cases = cases;
    this.interactif = interactif;
    this.onMot = onMot;
    this.onChange = onChange;
    this.chemin = [];
    this.rotation = 0;
    this.texteClavier = '';

    conteneur.innerHTML = `
      <div class="plateau-cadre">
        <div class="plateau t${taille}" style="grid-template-columns:repeat(${taille},1fr)">
          ${cases.map((c, i) => `<button class="case ${c.length > 1 ? 'qu' : ''}" data-i="${i}" tabindex="-1" aria-label="${c}">
            <span class="lettre ${AMBIGUES.has(c) ? 'souligne' : ''}">${c === 'QU' ? 'Qu' : c}</span></button>`).join('')}
          <svg class="trace"><polyline points=""/></svg>
        </div>
      </div>`;
    this.cadre = conteneur.querySelector('.plateau-cadre');
    this.grille = conteneur.querySelector('.plateau');
    this.elements = [...conteneur.querySelectorAll('.case')];
    this.trait = conteneur.querySelector('polyline');
    if (interactif) this.brancher();
    this.redimensionnement = new ResizeObserver(() => this.dessinerTrace());
    this.redimensionnement.observe(this.grille);
  }

  detruire() { this.redimensionnement.disconnect(); }

  get mot() { return this.chemin.map(i => this.cases[i]).join(''); }

  adjacentes(a, b) {
    const n = this.taille;
    return a !== b && Math.abs(Math.floor(a / n) - Math.floor(b / n)) <= 1 && Math.abs((a % n) - (b % n)) <= 1;
  }

  // Case sous le pointeur ; en mode "strict", il faut être près du centre (évite les diagonales ratées)
  caseSous(x, y, strict) {
    const el = document.elementFromPoint(x, y)?.closest('.case');
    if (!el || !this.grille.contains(el)) return null;
    if (strict) {
      const r = el.getBoundingClientRect();
      const dx = x - (r.left + r.width / 2), dy = y - (r.top + r.height / 2);
      if (Math.hypot(dx, dy) > r.width * 0.42) return null;
    }
    return Number(el.dataset.i);
  }

  brancher() {
    let glisse = null;
    let aValider = false;

    this.cadre.addEventListener('pointerdown', e => {
      const i = this.caseSous(e.clientX, e.clientY, false);
      if (i === null) return;
      e.preventDefault();
      this.texteClavier = '';
      const dernier = this.chemin.at(-1);
      if (this.chemin.length && i === dernier) aValider = true;                       // retouche la dernière lettre : valider
      else if (this.chemin.includes(i)) this.chemin = this.chemin.slice(0, this.chemin.indexOf(i) + 1); // revenir en arrière
      else if (this.chemin.length && this.adjacentes(dernier, i)) this.chemin.push(i); // lettre voisine : on continue
      else this.chemin = [i];                                                           // sinon on recommence
      glisse = { bouge: false };
      try { this.cadre.setPointerCapture(e.pointerId); } catch { /* pointeur déjà relâché */ }
      this.majSelection();
    });

    this.cadre.addEventListener('pointermove', e => {
      if (!glisse) return;
      const i = this.caseSous(e.clientX, e.clientY, true);
      if (i === null || i === this.chemin.at(-1)) return;
      if (this.chemin.length >= 2 && i === this.chemin.at(-2)) this.chemin.pop();
      else if (!this.chemin.includes(i) && this.adjacentes(this.chemin.at(-1), i)) this.chemin.push(i);
      else return;
      glisse.bouge = true;
      aValider = false;
      this.majSelection();
    });

    const fin = () => {
      if (!glisse) return;
      const valider = (glisse.bouge && this.chemin.length >= 2) || aValider;
      glisse = null;
      aValider = false;
      if (valider) this.valider();
    };
    this.cadre.addEventListener('pointerup', fin);
    this.cadre.addEventListener('pointercancel', () => { glisse = null; });
  }

  valider() {
    const mot = this.mot, chemin = [...this.chemin];
    this.effacer();
    if (mot) this.onMot(mot, chemin);
  }

  effacer() {
    this.chemin = [];
    this.texteClavier = '';
    this.majSelection();
  }

  // Saisie au clavier : on cherche un chemin qui écrit le texte tapé
  saisirTexte(texte) {
    this.texteClavier = texte;
    this.chemin = texte ? (this.cheminPour(texte) ?? []) : [];
    this.majSelection();
    return this.chemin.length > 0 || !texte;
  }

  cheminPour(mot) {
    const utilisees = new Array(this.cases.length).fill(false);
    const chemin = [];
    const explorer = (i, pos) => {
      const tuile = this.cases[i];
      if (utilisees[i] || !mot.startsWith(tuile, pos)) {
        // Au clavier, un "Q" seul en fin de saisie peut correspondre à la case "QU"
        if (!(tuile === 'QU' && pos === mot.length - 1 && mot[pos] === 'Q' && !utilisees[i])) return false;
      }
      utilisees[i] = true;
      chemin.push(i);
      const suite = pos + tuile.length;
      if (suite >= mot.length) return true;
      for (let v = 0; v < this.cases.length; v++)
        if (this.adjacentes(i, v) && explorer(v, suite)) return true;
      utilisees[i] = false;
      chemin.pop();
      return false;
    };
    for (let i = 0; i < this.cases.length; i++) if (explorer(i, 0)) return chemin;
    return null;
  }

  majSelection() {
    this.elements.forEach((el, i) => {
      el.classList.toggle('choisie', this.chemin.includes(i));
      el.classList.toggle('derniere', i === this.chemin.at(-1));
    });
    this.dessinerTrace();
    this.onChange(this.texteClavier || this.mot, this.chemin);
  }

  // Trace la sélection en cours, ou à défaut le mot montré dans les résultats
  dessinerTrace(chemin = this.chemin.length ? this.chemin : this.cheminMontre ?? []) {
    const cote = this.elements[0]?.offsetWidth || 0;
    this.trait.setAttribute('stroke-width', cote * 0.26);
    this.trait.setAttribute('points', chemin.map(i => {
      const el = this.elements[i];
      return `${el.offsetLeft + el.offsetWidth / 2},${el.offsetTop + el.offsetHeight / 2}`;
    }).join(' '));
  }

  // Met brièvement en couleur un chemin (mot valide, invalide ou montré dans les résultats)
  surligner(chemin, classe, duree = 450) {
    this.elements.forEach(el => el.classList.remove('ok', 'ko', 'repere'));
    chemin.forEach(i => this.elements[i].classList.add(classe));
    if (classe === 'repere' && !duree) {
      this.cheminMontre = chemin;
      this.dessinerTrace();
    }
    clearTimeout(this.minuteurSurligne);
    if (duree) this.minuteurSurligne = setTimeout(() => chemin.forEach(i => this.elements[i].classList.remove(classe)), duree);
  }

  // Comme avec un vrai plateau, on peut le tourner pour voir les mots autrement
  tourner() {
    this.rotation += 90;
    this.grille.style.transform = `rotate(${this.rotation}deg)`;
    this.grille.querySelectorAll('.lettre').forEach(l => { l.style.transform = `rotate(${-this.rotation}deg)`; });
  }
}
