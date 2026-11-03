#ifndef nethelp_h_i
#define nethelp_h_i

int kirjotaAsteroidi(char *msg, Pati *pati);
int lueAsteroidi(Pati *pati, char *msg);
int kirjotaPati(char *msg, Pati *pati);
int luePati(Pati *pati, char *msg);
int kirjotaAsteroidipox(char *msg, asteroidipox *ast);
int lueAsteroidipox(asteroidipox *ast, char *msg);
int kirjotaNetpati(char *msg, netPati *pati);
int lueNetpati(netPati *pati, char *msg);
int kirjotaKuolevapati(char *msg, kuolevaPati *pati);
int lueKuolevapati(kuolevaPati *pati, char *msg);
int kirjotaLimajuttu(char *msg, limajuttu *pati);
int lueLimajuttu(limajuttu *pati, char *msg);
int kirjotaCPiste(char *msg, CPiste *piste);
int lueCPiste(CPiste *piste, char *msg);
int kirjotaPikkumiessiirto(char *msg, pikkumiessiirto *siirto);
int luePikkumiessiirto(pikkumiessiirto *siirto, char *msg);
int kirjotaCoords(char *msg, Player *player);
int lueCoords(Player *player, char *msg);
int kirjotaSPiste(char *msg, SPiste *piste);
int lueSPiste(SPiste *piste, char *msg);

extern int spistekoko;
extern int coordkoko,pikkumieskoko,cpistekoko,limakoko;
extern int kuolevapatikoko, netpatikoko, patikoko,asteroidipoxkoko;
extern int asteroidikoko;

#endif