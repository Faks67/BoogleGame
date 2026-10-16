// Boggle : accueil, partie (solo, duel, défi du jour), résultats
import { api, esc, toast, reglages, records, defis } from './outils.js';
import { Plateau } from './plateau.js';

const ecran = document.getElementById('ecran');
const LANGUES = { FR: 'Français', EN: 'Anglais', MIX: 'Mixte' };
const DUREES = [60, 120, 180, 300];
const RAISONS = {
  'trop-court': 'Trop court : 3 lettres minimum',
  'absent-plateau': 'Impossible à former sur cette grille',
  'absent-dictionnaire': "Ce mot n'est pas dans le dictionnaire"
};

let partie = null;     // { mode, reglages, plateau (données serveur), joueurs, courant, date }
let nettoyage = null;  // arrête le chrono, le clavier, etc. de l'écran en cours

function afficher(html) {
  if (nettoyage) nettoyage();
  nettoyage = null;
  ecran.innerHTML = html;
  window.scrollTo(0, 0);
}

const formatChrono = s => `${Math.floor(s / 60)}:${String(s % 60).padStart(2, '0')}`;

// ======================= Accueil =======================
async function accueil() {
  const r = reglages.lire();
  let jour = null;
  try { jour = await api(`/api/parties/jour?langue=${r.langue}`); } catch { /* serveur indisponible */ }
  const defiJoue = jour ? defis.lire(jour.date, r.langue) : null;
  const liste = records.lire(r);

  afficher(`
    <section class="hero">
      <div class="des-deco">${'BOGGLE'.split('').map(l => `<span class="case"><span class="lettre">${l}</span></span>`).join('')}</div>
      <h1>Trouvez un max de <span>mots</span>.</h1>
      <p>Reliez des lettres voisines, à l'horizontale, à la verticale ou en diagonale, sans réutiliser un dé. Plus le mot est long, plus il rapporte.</p>
    </section>

    <div class="modes">
      <button class="carte mode" data-mode="solo">
        <span class="icone">1</span><h3>Partie solo</h3>
        <p class="muted small">Une grille, un chrono, battez votre record.</p>
      </button>
      <button class="carte mode" data-mode="duel">
        <span class="icone">2+</span><h3>Duel</h3>
        <p class="muted small">De 2 à 4 joueurs sur le même écran, chacun son tour. Un mot trouvé par plusieurs joueurs ne compte pas.</p>
      </button>
      <button class="carte mode" data-mode="defi">
        <span class="icone">★</span><h3>Défi du jour</h3>
        <p class="muted small">${defiJoue ? `Votre meilleur score aujourd'hui : <strong>${defiJoue.score} pt${defiJoue.score > 1 ? 's' : ''}</strong> (${defiJoue.mots} mot${defiJoue.mots > 1 ? 's' : ''}).` : 'La même grille pour tout le monde, nouvelle chaque jour. 4×4, 3 minutes.'}</p>
      </button>
    </div>

    <div class="carte"><div class="carte-corps reglages">
      ${segments('Langue', 'langue', Object.entries(LANGUES), r.langue)}
      ${segments('Grille', 'taille', [[4, '4×4'], [5, '5×5'], [6, '6×6']], r.taille)}
      ${segments('Durée', 'duree', DUREES.map(d => [d, `${d / 60} min`]), r.duree)}
    </div></div>

    <div class="deux-colonnes">
      <div class="carte"><div class="carte-corps">
        <h3 style="margin-bottom:10px">Records · ${LANGUES[r.langue]}, ${r.taille}×${r.taille}, ${r.duree / 60} min</h3>
        ${liste.length ? `<table><thead><tr><th>#</th><th>Joueur</th><th class="nombre">Mots</th><th class="nombre">Points</th><th>Date</th></tr></thead><tbody>
          ${liste.map((e, i) => `<tr><td>${i + 1}</td><td>${esc(e.nom)}</td><td class="nombre">${e.mots}</td><td class="nombre"><strong>${e.score}</strong></td><td class="muted small">${new Date(e.date).toLocaleDateString('fr-FR')}</td></tr>`).join('')}
        </tbody></table>` : '<p class="muted">Aucun record pour ces réglages. À vous de jouer !</p>'}
      </div></div>
      <div class="carte"><div class="carte-corps pile">
        <h3>Règles</h3>
        <details open><summary>Comment jouer</summary><ul>
          <li>Glissez sur les lettres voisines, ou touchez-les une à une puis retouchez la dernière pour valider.</li>
          <li>Au clavier : tapez le mot, <kbd>Entrée</kbd> pour valider, <kbd>Échap</kbd> pour effacer.</li>
          <li>Chaque dé ne sert qu'une fois par mot. « Qu » compte pour deux lettres.</li>
          <li>Tournez la grille pour voir les mots sous un autre angle.</li>
        </ul></details>
        <details><summary>Points</summary>
          <table style="margin-top:8px"><tbody>
            <tr><td>3 ou 4 lettres</td><td class="nombre">1 pt</td></tr><tr><td>5 lettres</td><td class="nombre">2 pts</td></tr>
            <tr><td>6 lettres</td><td class="nombre">3 pts</td></tr><tr><td>7 lettres</td><td class="nombre">5 pts</td></tr>
            <tr><td>8 lettres et plus</td><td class="nombre">11 pts</td></tr>
          </tbody></table>
        </details>
      </div></div>
    </div>`);

  ecran.querySelectorAll('.segments').forEach(seg => seg.addEventListener('click', e => {
    const b = e.target.closest('button');
    if (!b) return;
    const valeur = seg.dataset.cle === 'langue' ? b.dataset.valeur : Number(b.dataset.valeur);
    reglages.ecrire({ ...reglages.lire(), [seg.dataset.cle]: valeur });
    accueil();
  }));
  ecran.querySelector('[data-mode="solo"]').onclick = () => lancer('solo', [{ nom: 'Vous' }]);
  ecran.querySelector('[data-mode="duel"]').onclick = preparerDuel;
  ecran.querySelector('[data-mode="defi"]').onclick = () => lancer('defi', [{ nom: 'Vous' }], jour);
}

function segments(titre, cle, options, actuel) {
  return `<div class="reglage"><label>${titre}</label><div class="segments" data-cle="${cle}">
    ${options.map(([v, l]) => `<button data-valeur="${v}" class="${String(v) === String(actuel) ? 'actif' : ''}">${l}</button>`).join('')}
  </div></div>`;
}

// ======================= Duel : choix des joueurs =======================
function preparerDuel() {
  const r = reglages.lire();
  let noms = r.joueurs?.length >= 2 ? [...r.joueurs] : ['Joueur 1', 'Joueur 2'];
  const dessiner = () => {
    afficher(`
      <div class="carte" style="max-width:520px;margin:20px auto"><div class="carte-corps pile">
        <h2>Duel</h2>
        <p class="muted">Tout le monde joue sur la même grille, chacun son tour (${r.duree / 60} min). Les mots trouvés par plusieurs joueurs sont annulés.</p>
        ${noms.map((n, i) => `<div class="rangee" style="flex-wrap:nowrap">
          <input type="text" value="${esc(n)}" data-i="${i}" maxlength="20" aria-label="Nom du joueur ${i + 1}">
          ${noms.length > 2 ? `<button class="btn btn-fantome btn-icone" data-retirer="${i}" aria-label="Retirer">✕</button>` : ''}
        </div>`).join('')}
        <div class="rangee">
          ${noms.length < 4 ? '<button class="btn" id="ajouter">+ Ajouter un joueur</button>' : ''}
          <span class="espace"></span>
          <button class="btn btn-fantome" id="retour">Retour</button>
          <button class="btn btn-primaire" id="go">Lancer le duel</button>
        </div>
      </div></div>`);
    ecran.querySelectorAll('input').forEach(inp => inp.oninput = () => { noms[inp.dataset.i] = inp.value; });
    ecran.querySelectorAll('[data-retirer]').forEach(b => b.onclick = () => { noms.splice(Number(b.dataset.retirer), 1); dessiner(); });
    ecran.querySelector('#ajouter')?.addEventListener('click', () => { noms.push(`Joueur ${noms.length + 1}`); dessiner(); });
    ecran.querySelector('#retour').onclick = accueil;
    ecran.querySelector('#go').onclick = () => {
      noms = noms.map((n, i) => n.trim() || `Joueur ${i + 1}`);
      reglages.ecrire({ ...reglages.lire(), joueurs: noms });
      lancer('duel', noms.map(nom => ({ nom })));
    };
  };
  dessiner();
}

// ======================= Partie =======================
async function lancer(mode, joueurs, jour = null) {
  const r = reglages.lire();
  const regles = mode === 'defi' ? { langue: r.langue, taille: 4, duree: 180 } : { langue: r.langue, taille: r.taille, duree: r.duree };
  afficher('<p class="muted" style="text-align:center;padding:60px">On lance les dés…</p>');
  try {
    const donnees = mode === 'defi'
      ? (jour ?? await api(`/api/parties/jour?langue=${regles.langue}`))
      : { partie: await api('/api/parties', { langue: regles.langue, taille: regles.taille }) };
    partie = {
      mode, regles, plateau: donnees.partie, date: donnees.date,
      joueurs: joueurs.map(j => ({ nom: j.nom, mots: new Map() })), courant: 0
    };
  } catch (e) {
    afficher(`<div class="carte" style="max-width:520px;margin:40px auto"><div class="carte-corps pile"><h2>Oups</h2><p class="muted">${esc(e.message)}</p><div><button class="btn" id="retour">Retour</button></div></div></div>`);
    ecran.querySelector('#retour').onclick = accueil;
    return;
  }
  if (mode === 'duel') interlude(); else jouerTour();
}

function interlude() {
  const joueur = partie.joueurs[partie.courant];
  afficher(`
    <div class="carte interlude" style="max-width:560px;margin:30px auto">
      <p class="muted">Joueur ${partie.courant + 1} sur ${partie.joueurs.length}</p>
      <h2>Au tour de ${esc(joueur.nom)} !</h2>
      <p class="muted">Passez l'appareil sans regarder la grille. Vous aurez ${partie.regles.duree / 60} min.</p>
      <div style="margin-top:24px"><button class="btn btn-primaire btn-grand" id="pret">Je suis prêt</button></div>
    </div>`);
  ecran.querySelector('#pret').onclick = jouerTour;
}

function jouerTour() {
  const joueur = partie.joueurs[partie.courant];
  const { regles, plateau: p } = partie;
  const perimetre = 2 * Math.PI * 28;
  afficher(`
    <div class="jeu">
      <section class="zone-plateau">
        <div class="barre-jeu">
          <div class="chrono" id="chrono"><svg width="64" height="64" viewBox="0 0 64 64"><circle class="fond" cx="32" cy="32" r="28"/><circle class="avance" cx="32" cy="32" r="28" stroke-dasharray="${perimetre}" stroke-dashoffset="0"/></svg><span>${formatChrono(regles.duree)}</span></div>
          <div><div class="joueur-actif">${esc(partie.mode === 'duel' ? joueur.nom : partie.mode === 'defi' ? 'Défi du jour' : 'Partie solo')}</div>
            <div class="muted small">${LANGUES[regles.langue]} · ${regles.taille}×${regles.taille}</div></div>
          <span class="espace"></span>
          <div class="score-jeu"><strong id="score">0</strong><span class="muted small">points</span></div>
        </div>
        <div class="mot-courant" id="mot-courant"><span class="indice">Glissez sur les lettres ou tapez un mot</span></div>
        <div id="plateau" style="width:100%;display:flex;justify-content:center"></div>
        <div class="actions-jeu">
          <button class="btn" id="tourner">↻ Tourner la grille</button>
          <button class="btn" id="terminer">Terminer</button>
        </div>
        <p class="aide-clavier">Clavier : tapez le mot puis <kbd>Entrée</kbd> · <kbd>Échap</kbd> pour effacer</p>
      </section>
      <aside class="carte panneau-mots">
        <div class="entete-panneau">
          <div class="rangee"><strong id="compte">0 mot</strong><span class="espace"></span><span class="muted small">${p.nombreMots} possibles</span></div>
          <div class="progression"><span id="barre" style="width:0"></span></div>
        </div>
        <ul class="liste-mots" id="liste"></ul>
      </aside>
    </div>`);

  const zoneMot = ecran.querySelector('#mot-courant');
  const liste = ecran.querySelector('#liste');
  let score = 0;
  let messageJusqua = 0;

  const montrerMot = (texte, valide) => {
    if (Date.now() < messageJusqua) return;
    zoneMot.className = 'mot-courant';
    if (!texte) zoneMot.innerHTML = '<span class="indice">Glissez sur les lettres ou tapez un mot</span>';
    else {
      zoneMot.textContent = texte;
      if (valide === false) zoneMot.classList.add('ko');
    }
  };
  const message = (texte, classe) => {
    zoneMot.className = `mot-courant ${classe}${texte.length > 14 ? ' long' : ''}`;
    zoneMot.textContent = texte;
    messageJusqua = Date.now() + 900;
    setTimeout(() => { if (Date.now() >= messageJusqua) montrerMot(''); }, 950);
  };

  const proposer = async (mot, chemin) => {
    mot = mot.toUpperCase();
    if (mot.length < 3) { plateau.surligner(chemin, 'ko'); message(RAISONS['trop-court'], 'ko'); return; }
    if (joueur.mots.has(mot)) { plateau.surligner(chemin, 'repere'); message(`${mot} : déjà trouvé`, 'ko'); return; }
    try {
      const r = await api(`/api/parties/${encodeURIComponent(p.id)}/mots`, { mot });
      if (finie) return;
      if (!r.valide) {
        plateau.surligner(r.chemin ?? chemin, 'ko');
        message(RAISONS[r.raison] ?? 'Mot refusé', 'ko');
        return;
      }
      if (joueur.mots.has(r.mot)) return;
      joueur.mots.set(r.mot, r.points);
      score += r.points;
      plateau.surligner(r.chemin, 'ok');
      message(`+${r.points}  ${r.mot}`, 'ok');
      ecran.querySelector('#score').textContent = score;
      const n = joueur.mots.size;
      ecran.querySelector('#compte').textContent = `${n} mot${n > 1 ? 's' : ''}`;
      ecran.querySelector('#barre').style.width = `${Math.min(100, (n / p.nombreMots) * 100)}%`;
      liste.querySelector('.nouveau')?.classList.remove('nouveau');
      liste.insertAdjacentHTML('afterbegin', `<li class="nouveau"><span>${esc(r.mot)}</span><span class="pts">+${r.points}</span></li>`);
    } catch (e) {
      message(e.message, 'ko');
    }
  };

  const plateau = new Plateau(ecran.querySelector('#plateau'), {
    taille: p.taille, cases: p.cases, onMot: proposer,
    onChange: (texte, chemin) => montrerMot(texte, texte ? chemin.length > 0 : undefined)
  });

  // ----- Clavier -----
  const touche = e => {
    if (e.ctrlKey || e.metaKey || e.altKey) return;
    if (e.key === 'Enter') { e.preventDefault(); const t = plateau.texteClavier || plateau.mot; const c = [...plateau.chemin]; plateau.effacer(); if (t) proposer(t, c); }
    else if (e.key === 'Escape') plateau.effacer();
    else if (e.key === 'Backspace') { e.preventDefault(); plateau.saisirTexte(plateau.texteClavier.slice(0, -1)); }
    else if (/^[a-zA-Z]$/.test(e.key)) plateau.saisirTexte((plateau.texteClavier + e.key).toUpperCase());
  };
  document.addEventListener('keydown', touche);

  // ----- Chrono -----
  let finie = false;
  const fin = Date.now() + regles.duree * 1000;
  const chrono = ecran.querySelector('#chrono');
  const anneau = chrono.querySelector('.avance');
  const tic = () => {
    const reste = Math.max(0, Math.ceil((fin - Date.now()) / 1000));
    chrono.querySelector('span').textContent = formatChrono(reste);
    anneau.setAttribute('stroke-dashoffset', perimetre * (1 - reste / regles.duree));
    chrono.classList.toggle('urgent', reste <= 10);
    if (reste === 0) terminer();
  };
  const minuteur = setInterval(tic, 250);

  const terminer = () => {
    if (finie) return;
    finie = true;
    if (partie.mode === 'duel' && partie.courant < partie.joueurs.length - 1) {
      partie.courant++;
      toast(`Temps écoulé pour ${joueur.nom} !`);
      interlude();
    } else {
      resultats();
    }
  };

  ecran.querySelector('#tourner').onclick = () => plateau.tourner();
  ecran.querySelector('#terminer').onclick = () => { if (confirm('Terminer maintenant ?')) terminer(); };

  nettoyage = () => {
    finie = true;
    clearInterval(minuteur);
    document.removeEventListener('keydown', touche);
    plateau.detruire();
  };
}

// ======================= Résultats =======================
async function resultats() {
  const { mode, regles, plateau: p, joueurs } = partie;
  afficher('<p class="muted" style="text-align:center;padding:60px">Calcul des résultats…</p>');
  let solutions = [];
  try { solutions = await api(`/api/parties/${encodeURIComponent(p.id)}/solutions`); } catch { /* on affiche sans les solutions */ }

  // Un mot trouvé par plusieurs joueurs ne rapporte rien (règle officielle)
  const compte = new Map();
  joueurs.forEach(j => j.mots.forEach((_, m) => compte.set(m, (compte.get(m) || 0) + 1)));
  const multi = joueurs.length > 1;
  joueurs.forEach(j => {
    j.score = [...j.mots].reduce((s, [m, pts]) => s + (multi && compte.get(m) > 1 ? 0 : pts), 0);
    j.meilleur = [...j.mots.keys()].sort((a, b) => b.length - a.length || a.localeCompare(b))[0] || '—';
  });
  const classement = [...joueurs].sort((a, b) => b.score - a.score);
  const trouvesPar = new Set(compte.keys());

  let entete;
  if (!multi) {
    const j = joueurs[0];
    let distinction = '';
    if (mode === 'solo') {
      const place = records.ajouter(regles, { nom: 'Vous', score: j.score, mots: j.mots.size, date: Date.now() });
      if (place === 1 && j.score > 0) distinction = '<span class="badge badge-or">Nouveau record !</span>';
      else if (place) distinction = `<span class="badge">${place}e meilleur score</span>`;
    } else {
      const avant = defis.lire(partie.date, regles.langue);
      defis.ecrire(partie.date, regles.langue, { score: j.score, mots: j.mots.size });
      if (!avant || j.score > avant.score) distinction = '<span class="badge badge-or">Meilleur score du jour</span>';
    }
    entete = `
      <div class="resultats-tete pile" style="align-items:center">
        ${distinction}
        <h1>${j.score} point${j.score > 1 ? 's' : ''}</h1>
        <p class="muted">${mode === 'defi' ? `Défi du ${new Date(partie.date).toLocaleDateString('fr-FR')}` : 'Partie solo'} · ${LANGUES[regles.langue]} · ${regles.taille}×${regles.taille}</p>
      </div>
      <div class="stats">
        <div class="carte stat"><span class="muted small">Mots trouvés</span><strong>${j.mots.size} / ${p.nombreMots}</strong></div>
        <div class="carte stat"><span class="muted small">Points possibles</span><strong>${Math.round((j.score / Math.max(1, p.pointsMax)) * 100)} %</strong></div>
        <div class="carte stat"><span class="muted small">Votre plus long mot</span><strong>${esc(j.meilleur)}</strong></div>
        <div class="carte stat"><span class="muted small">Le plus long possible</span><strong>${esc(solutions[0]?.mot ?? '—')}</strong></div>
      </div>`;
  } else {
    const egalite = classement.length > 1 && classement[0].score === classement[1].score;
    entete = `
      <div class="resultats-tete"><h1>${egalite ? 'Égalité !' : `${esc(classement[0].nom)} gagne !`}</h1>
        <p class="muted">Les mots barrés ont été trouvés par plusieurs joueurs et ne rapportent rien.</p></div>
      <div class="podium">${classement.map((j, i) => `
        <div class="carte place ${i === 0 && !egalite ? 'premier' : ''}">
          <span class="rang">${i + 1}</span>
          <div><strong>${esc(j.nom)}</strong>
            <div class="small" style="margin-top:4px;display:flex;flex-wrap:wrap;gap:4px 10px">
              ${[...j.mots.keys()].sort().map(m => `<span class="${compte.get(m) > 1 ? 'mot-barre' : ''}">${esc(m)}</span>`).join('') || '<span class="muted">Aucun mot</span>'}
            </div></div>
          <strong class="points">${j.score}</strong>
        </div>`).join('')}</div>`;
  }

  afficher(`
    ${entete}
    <div class="rangee" style="justify-content:center;margin-bottom:24px">
      ${mode === 'defi' ? '' : '<button class="btn btn-primaire btn-grand" id="rejouer">Rejouer</button>'}
      <button class="btn btn-grand" id="accueil">Accueil</button>
    </div>
    <div class="carte"><div class="carte-corps">
      <h3 style="margin-bottom:14px">Tous les mots de la grille (${solutions.length})</h3>
      <div class="revue">
        <div id="revue-plateau"></div>
        <div>
          <div class="filtres-mots">
            <button class="puce active" data-filtre="tous">Tous</button>
            <button class="puce" data-filtre="trouves">Trouvés (${solutions.filter(s => trouvesPar.has(s.mot)).length})</button>
            <button class="puce" data-filtre="manques">Manqués (${solutions.filter(s => !trouvesPar.has(s.mot)).length})</button>
          </div>
          <p class="muted small" style="margin-bottom:10px">Touchez un mot pour voir où il se cache.</p>
          <div class="nuage" id="nuage"></div>
        </div>
      </div>
    </div></div>`);

  const revue = new Plateau(ecran.querySelector('#revue-plateau'), { taille: p.taille, cases: p.cases, interactif: false });
  const nuage = ecran.querySelector('#nuage');
  const dessinerNuage = filtre => {
    const liste = solutions.filter(s => filtre === 'tous' || (filtre === 'trouves') === trouvesPar.has(s.mot));
    nuage.innerHTML = liste.map(s => `<button class="mot-sol ${trouvesPar.has(s.mot) ? 'trouve' : ''}" data-mot="${esc(s.mot)}">${esc(s.mot)}<small>${s.points}</small></button>`).join('')
      || '<p class="muted">Aucun mot.</p>';
  };
  dessinerNuage('tous');
  ecran.querySelector('.filtres-mots').onclick = e => {
    const b = e.target.closest('[data-filtre]');
    if (!b) return;
    ecran.querySelectorAll('[data-filtre]').forEach(x => x.classList.toggle('active', x === b));
    dessinerNuage(b.dataset.filtre);
  };
  nuage.onclick = e => {
    const b = e.target.closest('[data-mot]');
    if (!b) return;
    nuage.querySelectorAll('.actif').forEach(x => x.classList.remove('actif'));
    b.classList.add('actif');
    revue.surligner(solutions.find(s => s.mot === b.dataset.mot).chemin, 'repere', 0);
  };

  ecran.querySelector('#accueil').onclick = accueil;
  ecran.querySelector('#rejouer')?.addEventListener('click', () =>
    mode === 'duel' ? lancer('duel', joueurs.map(j => ({ nom: j.nom }))) : lancer('solo', [{ nom: 'Vous' }]));
  nettoyage = () => revue.detruire();
}

// ======================= Thème et démarrage =======================
const CLE_THEME = 'boggle.theme';
try {
  const theme = localStorage.getItem(CLE_THEME);
  if (theme) document.documentElement.dataset.theme = theme;
} catch { /* stockage indisponible */ }
document.getElementById('btn-theme').onclick = () => {
  const sombre = matchMedia('(prefers-color-scheme: dark)').matches;
  const actuel = document.documentElement.dataset.theme || (sombre ? 'dark' : 'light');
  const nouveau = actuel === 'dark' ? 'light' : 'dark';
  document.documentElement.dataset.theme = nouveau;
  try { localStorage.setItem(CLE_THEME, nouveau); } catch { /* stockage indisponible */ }
};
document.getElementById('logo').onclick = () => {
  if (partie && ecran.querySelector('#chrono') && !confirm('Quitter la partie en cours ?')) return;
  accueil();
};

accueil();
