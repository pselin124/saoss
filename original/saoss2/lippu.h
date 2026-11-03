#ifndef lippu_h
#define lippu_h
#define MAA 101
#define TANKO 102
#define KUKKA5 103
#define SIENI2 104
#define	KUKKA22 105
#define KUKKA33 106
#define KUKKA44 107
#define VARJOKUKKA5 108
#define VARJOSIENI2 109
#define VARJOKUKKA22 110
#define VARJOKUKKA33 111
#define VARJOKUKKA44 112
#define NUOLI2 113

#define PERSPEKTIIVI 65

extern int kukka4;
extern int kukka3;
extern int kukka2;
extern int sieni;
extern int kukka;
extern GLfloat rotX, rotY, dist;
extern int lipputekstuuri;
extern int detail;

void loadTextures();
extern GLuint texturet[];
extern int texturecount;
extern int screenwidth, screenheight;
extern GLfloat light0position[];
void Reshape( int width, int height );
void Init( void );
void Display(void);

#define LIPUN_X_VERTEXIT 35//35//13 //55
#define LIPUN_Y_VERTEXIT 25//25//10 //39
/*#define LIPUN_LEVEYS 5.6
#define LIPUN_KORKEUS 4 
#define gravitaatiovakio 0.055f
#define TANGON_KORKEUS 15*/
//#define LIPUN_X_VERTEXIT 24//35//13 //55
//#define LIPUN_Y_VERTEXIT 18//25//10 //39
#define LIPUN_LEVEYS 6.0f
#define LIPUN_KORKEUS 4.5f 
#define gravitaatiovakio 0.055f
#define TANGON_KORKEUS 15.0f

typedef struct vertex{
  GLfloat position[3];
  float force[3], v[3];
  struct vertex *naapurit[8];
  int naapureidenMaara;
  int naapureidenMaara2;
  float texCoord[2];
  int kolmiomaara;
  int kolmiot[6];
  float normal[3];
}vertex;

typedef struct{
  vertex *kulmat[3];
  float normal[3];
}triangle;

extern int framecounter;
extern vertex *vertexit;
extern triangle *trianglet;
extern float tuulenvoimakkuus, tuulikulma;

void drawGround();
void drawPole();
void calculate(int framecount);
void draw();




#endif