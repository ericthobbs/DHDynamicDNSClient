#
#Makefile for DHDynDNSAgent
#Version 1 - 5/22/13
#
CXX := clang++
DEBUG = -g -DDEBUG -D_GLIBCXX_USE_NANOSLEEP -O1
CXXFLAGS = -Wall -std=c++11 $(DEBUG) -c
LFLAGS = -Wall -std=c++11 $(DEBUG)

PREFIX :=
BINDIR := $(PREFIX)/bin/
ETCDIR := $(PREFIX)/etc/
SYSDIR := $(PREFIX)/etc/systemd/system/

DEP_CFLAGS = $(shell pkg-config --cflags xerces-c libcurl uuid)
DEP_LIBS   = $(shell pkg-config --libs   xerces-c libcurl uuid)

CXXFLAGS += $(DEP_CFLAGS)
LFLAGS += -lpopt $(DEP_LIBS)

OBJS_AGENT = agent.o daemon.o configreader.o parameters.o httpclient.o \
		httpclientexception.o configfileexception.o configsaxhandler.o \
		malformedxmlexception.o dhxmlreader.o dhsaxhandler.o util.o \
		dhapikeypermissionexception.o dhapirecordnotfoundexception.o

OBJS_IP = ip.o configreader.o parameters.o httpclient.o \
		httpclientexception.o configfileexception.o configsaxhandler.o \
		util.o malformedxmlexception.o

DEPS = $(shell ls *.hpp)

DHDynDNSAgent : $(OBJS_AGENT)
	$(CXX) $(LFLAGS) $(OBJS_AGENT) -o $@

whatsmyip : $(OBJS_IP)
	$(CXX) $(LFLAGS) $(OBJS_IP) -o $@

%.o: %.cpp $(DEPS)
	$(CXX) $(CXXFLAGS) -c -o $@ $<

.PHONY: clean

clean:
	\rm *.o DHDynDNSAgent
	\rm -fr apidocs

docs:
	doxygen

install:
	#Install Binaries
	mkdir -p $(BINDIR)
	cp DHDynDNSAgent $(BINDIR)
	cp whatsmyip $(BINDIR)
	#Install Configurtion file
	mkdir -p $(ETCDIR)dhdns/
	cp ../config/etc/config.xml $(ETCDIR)dhdns/
	@echo "Bootscripts not installed."
	@echo "run install-systemd if you have a systemd compatible system"

install-systemd:
	cp ../config/systemd/dhdns.service $(SYSDIR)

setup-system:
	yum install xerces-c-devel libcurl-devel \
		libuuid-devel boost-devel popt-devel \
		libstdc++ libstdc++-static clang

