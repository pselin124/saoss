 
#include "teksti.h"

/*
 * Näytettävät tekstit
 */
char *texts[] = {"Player 1 movement: left, right and up arrow keys",
                 "Player 1 kick: JUMP + END",
		             "Player 1 hit: HOME",
		             "Player 2 movement: left = A, right = D, jump = W",
		             "Player 2 kick: JUMP + 1",
		             "Player 1 hit: 2",
		             "Texture mode on/off: T",
		             "Wireframe mode on/off: L",
		             "Blur mode on/off: B",
		             "Real Arena on/off: R",
		             "Help Screen on/off: H",
		             "ESC exits, right mouse button displays some options."};

void drawString(float x, float y, char *string)
{
  int length;
  
  length = strlen(string);
  glRasterPos2f(x, y);
  for (int i = 0; i < length; i++)

	 glBindTexture(GL_TEXTURE_2D, texture[0]);				// Select Our Texture

   
   
	double koko=1.0f;
	glBegin(GL_QUADS);
		// Front Face
//		glColor3f(1,1,1);
//		glColor4f(1.0f,1.0f,1.0f, 0.9f);		

//		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 0.0f); glVertex2f(-1.0f*koko, -1.0f*koko);	
		glTexCoord2f(1.0f, 0.0f); glVertex2f( koko, -1.0f*koko);	
		glTexCoord2f(1.0f, 1.0f); glVertex2f( koko,  koko);	
		glTexCoord2f(0.0f, 1.0f); glVertex2f(-1.0f*koko,  koko);	
	glEnd();
//    glutBitmapCharacter(GLUT_BITMAP_9_BY_15, string[i]);
} 

/*
 * Asettaa matriisit sopivaan tilaan ja kutsuu drawStringiä helppiteksteille.
 */
void drawAlku(void){
  /* ensimmäisen tekstin x-koordinaatti, y-koordinaatti ja "rivinväli" */
  double x=-0.95, y=0.90, dy = 0.1;

  /* valot ja teksturoinnit hetkeksi pois päältä, jotta saadaan jotain näkymään */
  glDisable(GL_LIGHTING);
  glDisable(GL_TEXTURE_2D);

  /* vaihdetaan matriisimoodia ja pushataan uusi matriisi käyttöön */
  glMatrixMode(GL_PROJECTION);
  glPushMatrix();

  /* matriisin alustus */
  glLoadIdentity();
  
  /* Muutetaan projektio hetkeksi paralleeliprojektioksi teksitä varten */
  glOrtho(0.0, (double)640, 0.0, (double)480, -0.0, 0.0);
  
  /* Vaihdtaan taas moodia ja alustetaan */ 
  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity();

  /* mukava harmahtava väri tekstille */
  glColor3ub(200, 200, 200);

  /* varsinainen piirto, vähennetään y:tä aina seuraavaa riviä varten */
  for (int i=0; i<12; i++, y -=dy){
    drawString(x, y, texts[i]);
  }

  /* paluu normaalitilaan */
  glMatrixMode(GL_PROJECTION);
  glPopMatrix();
  glMatrixMode(GL_MODELVIEW);
  glEnable(GL_LIGHTING);
  glEnable(GL_TEXTURE_2D);
}
