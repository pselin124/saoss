CC = gcc
CFLAGS = -O3 -g `sdl-config --cflags` -I.

LIBS = `sdl-config --libs` -lGL -lGLU \
	-lSDL_image -lSDL_net -lfmod-3.62

SRC = Alus.cpp Alustuksia.cpp Main.cpp Menu.cpp Player.cpp Sets.cpp \
      Teksti.cpp aluspiirto.cpp audio.cpp chat.cpp glapi.cpp ctf.cpp \
      domination.cpp lippu.cpp localgame.cpp net.cpp scene.cpp \
      netav.cpp netgamesets.cpp nethelp.cpp netpeli.cpp \
      peliloppu.cpp sieni.cpp dmenu.cpp

OBJ = $(SRC:%.cpp=src/%.o)

all: saoss

clean:
	rm -f $(OBJ)

distclean: clean
	rm -f saoss

saoss: $(OBJ)
	$(CC) $(CFLAGS) -o $@ $(OBJ) $(LIBS)

%.o: %.cpp
	$(CC) $(CFLAGS) -c -o $@ $<

