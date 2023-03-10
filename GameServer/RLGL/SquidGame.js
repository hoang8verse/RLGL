#!/usr/bin/env node
var WebSocketServer = require('websocket').server;
const otpGenerator = require('otp-generator');

const { json } = require('express');
var http = require('http');
var https = require('https');
var fs = require('fs');

var server = http.createServer(function(request, response) {
    console.log((new Date()) + ' Received request for ' + request.url);
    response.writeHead(404);
    response.end();
});

// const server = https.createServer({
//     cert: fs.readFileSync('rootSSL.pem'),
//     key: fs.readFileSync('key.pem')
//   });

server.listen(8081, function() {
    console.log((new Date()) + ' Server is listening on port 8081');
});

wsServer = new WebSocketServer({
    httpServer: server,
    // You should not use autoAcceptConnections for production
    // applications, as it defeats all standard cross-origin protection
    // facilities built into the protocol and the browser.  You should
    // *always* verify the connection's origin and decide whether or not
    // to accept it.
    autoAcceptConnections: false
});

function originIsAllowed(origin) {
  // put logic here to detect whether the specified origin is allowed.
  return true;
}
const RoomStores = [];
function getRoom(len, roomId = null) {
    console.log("RoomStores before -------------  ", RoomStores)
    let roomRan = otpGenerator.generate(6, {
        // digits: true,
        upperCaseAlphabets: false,
        specialChars: false,
    });
    if(RoomStores.length == 0){
        RoomStores.push({
            room : roomId ? roomId : roomRan,
            numPlayer : 1
        })
    } else {
        let lastRoom = RoomStores[RoomStores.length - 1];
        if(lastRoom.numPlayer < len){
            RoomStores[RoomStores.length - 1].numPlayer += 1;
        } else {
            RoomStores.push({
                room : roomId ? roomId : roomRan,
                numPlayer : 1
            })
        }
    }
    console.log("RoomStores afterrrr -------------  ", RoomStores)
    return RoomStores[RoomStores.length - 1].room;
  }

const rooms = {};
var Player = require('./PlayerSquidGame.js');
function str2ab(str) {
    var buf = new ArrayBuffer(str.length*2); // 2 bytes for each char
    var bufView = new Uint8Array(buf);
    for (var i=0, strLen=str.length; i<strLen; i++) {
      bufView[i] = str.charCodeAt(i);
    }
    return buf;
  }
function parseVector3(str) {
    console.log("str ============  " , str);
    var sliceString = str.slice(1,str.length - 1);
    console.log("sliceString ============  " , sliceString);
    return sliceString.split(',');
  }
wsServer.on('request', function(request) {
    if (!originIsAllowed(request.origin)) {
      // Make sure we only accept requests from an allowed origin
      request.reject();
      console.log((new Date()) + ' Connection from origin ' + request.origin + ' rejected.');
      return;
    }

    // remove echo-protocol
    var connection = request.accept("", request.origin);
    // var connection = request.accept( request.origin);
    console.log((new Date()) + ' Connection accepted.');
    const clientId = otpGenerator.generate(6, {
        // digits: true,
        upperCaseAlphabets: false,
        specialChars: false,
    });
    console.log("clientId ===================   " ,clientId);
    const leave = room => {
        console.log("_room leave leave ===========  " , room)
        // not present: do nothing
        if(! rooms[room][clientId]) return;
        let checkNewHost = "";
        // if the one exiting is the last one, destroy the room
        if(Object.keys(rooms[room]).length === 1){
            // delete room name
            // RoomStores.forEach(roomPlayer => {
            //     if(roomPlayer.room == room){
            //         var index = RoomStores.indexOf(roomPlayer);
            //         if (index > -1) {
            //             RoomStores.splice(index, 1);
            //         }
            //     }
            // });


            delete rooms[room];
        }
        // otherwise simply leave the room
        else {
            if(rooms[room][clientId]["player"]["isHost"] == "1"){
                Object.entries(rooms[room]).forEach(([, sock]) => {
                    console.log("leave leave sock aaaa ad=====  " , sock);
                    if(sock["player"]["id"] != clientId){
                        rooms[room][sock["player"]["id"]]["isHost"] = "1";
                        checkNewHost = sock["player"]["id"];
                        return;
                    }
                });
            }

            delete rooms[room][clientId];
        }
        if(rooms[room]) {

            Object.entries(rooms[room]).forEach(([, sock]) => {
                console.log("leave leave sock =====  " , sock["player"]);
                let params = {
                    event : "playerLeaveRoom",
                    clientId : clientId,
                    newHost : checkNewHost,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                sock.sendBytes(buffer);
            });
        }

    };


    connection.on('message', function(message) {
        if (message.type === 'utf8') {
            // plant text
        }
        else if (message.type === 'binary') {
            // console.log('Received Binary Message of ' + message.binaryData + ' bytes');
            // connection.sendBytes(message.binaryData);

            var data = JSON.parse(message.binaryData);
            console.log('Received Message binary :  ' +  message.binaryData);
            const { meta, room } = data;

            if(meta === "requestRoom") {
                console.log("playerLen =========== binary  " , parseInt(data.playerLen))

                let host = data['host'];
                // let _room = getRoom(parseInt(data.playerLen), room);
                let _room;
                if(host == "1"){
                    _room = room;
                } else {
                    _room = room.substring(0,room.length-1);
                    if(!rooms[_room]){
                        let params = {
                            event : "failJoinRoom",
                            clientId : clientId,
                            room : _room,
                            message : "Room id : " + _room + " is not availiable! Please try again.",
                        }
                        let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                        connection.sendBytes(buffer);

                        return;
                    }
                }
                // console.log("_room nameeeeeeeeeee _room ===========  " , _room.length)
                // console.log("_room nameeeeeeeeeee adadadad ===========  " , room.length)
                // for (let index = 0; index < room.length; index++) {
                //     console.log(" roomm text id ===========  " , room[index])
                // }
                // if(_room == room){
                //     console.log("_room correct rooommmm adadadad ===========  " )
                // }

                let params = {
                    event : "roomDetected",
                    clientId : clientId,
                    room : _room,
                }
                // let bufferArr = str2ab(JSON.stringify(params));
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');

                connection.sendBytes(buffer);
            }
            else if(meta === "joinLobby") {

                console.log(' rooms[room] before ========  ' , rooms);

                if(!rooms[room]){
                    rooms[room] = {}; // create the room
                    // console.log(" created new aaaaaaaaaaaa room ===========  " , rooms)

                }
                if(! rooms[room][clientId]) rooms[room][clientId] = connection; // join the room
                // console.log(' rooms[room] 111111111111 ========  ' , rooms);

                var player = new Player();
                player.id = clientId;
                // player.playerName = "Player " + Object.keys(rooms[room]).length;
                player.playerName = data.playerName;
                player.room = room;
                console.log( "  new player created  ----------- " , player)
                // let _pos = parseVector3(data.pos);
                // console.log("pos :   " , _pos);
                // player.position = _pos;

                rooms[room][clientId]["player"] = player;// save player in room array
                let players = [];
                Object.entries(rooms[room]).forEach(([, sock]) => {
                    // console.log( "  sock ----------- " , sock.player)
                    players.push(sock.player);
                });
                player.isHost = data.isHost;
                // if(players.length == 1){
                //     player.isHost = "1";
                // } else {
                //     player.isHost = "0";
                // }
                let params = {
                    event : "joinLobbyRoom",
                    clientId : clientId,
                    playerName : player.playerName,
                    players : players,
                    isHost : player.isHost,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => {
                    console.log( "  sock ----------- " , sock.player)

                    sock.sendBytes(buffer);
                });

            }
            else if(meta === "gotoGame") {

                console.log("gotoGame  data ===========  " , data)
                let params = {
                    event : "gotoGame",
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                console.log("gotoGame  buffer========  " , buffer)
                // console.log("startGame  rooms[room]========  " , rooms[room])
                Object.entries(rooms[room]).forEach(([, sock]) => {
                   sock.sendBytes(buffer)
                });
            }
            else if(meta === "join") {

                // console.log(' clientId ========  ' , clientId);

                if(! rooms[room]){
                    rooms[room] = {}; // create the room
                }
                if(! rooms[room][clientId]) rooms[room][clientId] = connection; // join the room

                var player = new Player();
                player.id = clientId;
                // player.playerName = "Player " + Object.keys(rooms[room]).length;
                player.playerName = data.playerName;
                player.room = room;
                console.log( "  new player created  ----------- " , player)
                let _pos = parseVector3(data.pos);
                console.log("pos :   " , _pos);
                player.position = _pos;

                rooms[room][clientId]["player"] = player;// save player in room array
                let players = [];
                Object.entries(rooms[room]).forEach(([, sock]) => {
                    // console.log( "  sock ----------- " , sock.player)
                    players.push(sock.player);
                });
                player.isHost = data.isHost;
                // if(players.length == 1){
                //     player.isHost = "1";
                // } else {
                //     player.isHost = "0";
                // }
                let params = {
                    event : "joinRoom",
                    clientId : clientId,
                    playerName : player.playerName,
                    players : players,
                    isHost : player.isHost,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => {
                    console.log( "  sock ----------- " , sock.player)

                    sock.sendBytes(buffer);
                });

            }
            else if(meta === "startGame") {

                console.log("startGame  data ===========  " , data)
                let params = {
                    event : "startGame",
                    clientId : clientId,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                console.log("startGame  buffer========  " , buffer)
                // console.log("startGame  rooms[room]========  " , rooms[room])
                Object.entries(rooms[room]).forEach(([, sock]) => {
                   sock.sendBytes(buffer)
                });
            }
            else if(meta === "moving") {

                console.log("moving moving data ===========  " , data)
                let params = {
                    event : "moving",
                    clientId : clientId,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => {
                   sock.sendBytes(buffer)
                });

            }
            else if(meta === "stopMove") {
                console.log("stopMove stopMove data ===========  " , data)
                let params = {
                    event : "stopMove",
                    clientId : clientId,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => sock.sendBytes(buffer));
            }
            else if(meta === "headTurn") {
                console.log("headTurn headTurn ===========  " , data)
                const rnd = Math.random() * 3 + 1;
                let params = {
                    event : "headTurn",
                    clientId : clientId,
                    speedHeadTurn : rnd,
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => sock.sendBytes(buffer));
            }
            else if(meta === "playerDie") {
                console.log("playerDie data ========================= " + data);
                rooms[room][clientId]["player"]["playerStatus"] = "die";
                let params = {
                    event : "playerDie",
                    clientId : clientId,
                    playerStatus :  "die"
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => sock.sendBytes(buffer));

            }
            else if(meta === "playerWin") {
                rooms[room][clientId]["player"]["playerStatus"] = "win";
                let params = {
                    event : "playerWin",
                    clientId : clientId,
                    playerStatus :  "win"
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => sock.sendBytes(buffer));
            }
            else if(meta === "endGame") {

                let players = [];
                Object.entries(rooms[room]).forEach(([, sock]) => {
                    players.push(sock.player);
                });
                // console.log("players  array ====================== " + players);
                let params = {
                    event : "endGame",
                    clientId : clientId,
                    players :  players
                }
                let buffer = Buffer.from(JSON.stringify(params), 'utf8');
                Object.entries(rooms[room]).forEach(([, sock]) => sock.sendBytes(buffer));
            }
            else if(meta === "leave") {

                leave(room);

            }
            else if(! meta) {
                // send the message to all in the room

                // Object.entries(rooms[room]).forEach(([, sock]) => sock.sendBytes( JSON.stringify(param) ));
            }

        }
    });

    connection.on('close', function(reasonCode, description) {
        console.log((new Date()) + ' Peer ' + connection.remoteAddress + ' disconnected.');
        // for each room, remove the closed socket
        Object.keys(rooms).forEach(room => leave(room));
    });
});