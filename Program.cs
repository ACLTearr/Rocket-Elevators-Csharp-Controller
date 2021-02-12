using System;
using System.Collections.Generic;
using System.Linq;

namespace Commercial_Controller {
    
    //Defining Battery class
    public class Battery {
        public int ID;
        public int amountOfColumns;
        public string status;
        public int amountOfFloors;
        public int amountOfBasements;
        public int amountOfElevatorPerColumn;
        public int[] servedFloors;
        public int columnID = 1;
        public int floorRequestButtonID = 1;
        public List<Column> columnsList;
        public List<FloorRequestButton> floorRequestButtonsList;

        public Battery(int ID, int amountOfColumns, string status, int amountOfFloors, int amountOfBasements, int amountOfElevatorPerColumn) {
            this.ID = ID;
            this.amountOfColumns = amountOfColumns;
            this.status = status;
            this.amountOfFloors = amountOfFloors;
            this.amountOfBasements = amountOfBasements;
            this.amountOfElevatorPerColumn = amountOfElevatorPerColumn;
            this.columnsList = new List<Column>();
            this.floorRequestButtonsList = new List<FloorRequestButton>();

            if (amountOfBasements > 0) {
                this.makeBasementFloorRequestButtons(amountOfBasements);
                this.makeBasementColumn(amountOfBasements, amountOfElevatorPerColumn);
                amountOfColumns--;
            }

            this.makeFloorRequestButtons(amountOfFloors);
            this.makeColumns(amountOfColumns, amountOfFloors, amountOfElevatorPerColumn);

            for (int i = 0; i < amountOfColumns + 1; i++) {
                columnsList[i].makeCallButtons(amountOfFloors, amountOfBasements, columnsList[i].isBasement);
            }

            for (int i = 0; i < amountOfColumns + 1; i++) {
                columnsList[i].makeElevators(columnsList[i].servedFloorsList, columnsList[i].amountOfElevators);
            }
        }

        //Method to create basement columns
        public void makeBasementColumn(int amountOfBasements, int amountOfElevatorPerColumn) {
            int[] servedFloors = new int[amountOfBasements + 1];
            int floor = -1;
            for (int i = 0; i < (amountOfBasements + 1); i++) {
                if (i == 0) { //Adding main floor to floor list
                    servedFloors[i] = 1;
                } else {
                    servedFloors[i] = floor;
                    floor--;
                }
            }
            var column = new Column(columnID, "online", amountOfElevatorPerColumn, servedFloors, true);
            columnsList.Add(column);
            columnID++;
        }
            
        //Method to create columns
        public void makeColumns(int amountOfColumns, int amountOfFloors, int amountOfElevatorPerColumn) {
            int amountOfFloorsPerColumn = (int)Math.Ceiling((double)amountOfFloors / amountOfColumns);
            int floor = 1;
            for (int i = 0; i < amountOfColumns; i++) {
                int[] servedFloors = new int[amountOfFloorsPerColumn + 1];
                for (int x = 0; x < amountOfFloorsPerColumn; x++) {
                    if (i == 0) { //For first above ground column
                        servedFloors[x] = floor;
                        floor++;
                    } else { //For all columns after first above ground, to make sure main floor is included
                        servedFloors[0] = 1;
                        servedFloors[x + 1] = floor;
                        floor++;
                    }
                }
                var column = new Column(columnID, "online",amountOfElevatorPerColumn, servedFloors, false);
                columnsList.Add(column);
                columnID++; 
            }
        }

        //Method to create basement floor request buttons
        public void makeBasementFloorRequestButtons(int amountOfBasements) {
            int buttonFloor = -1;
            for (int i = 0; i < amountOfBasements; i++) {
                var floorRequestButton = new FloorRequestButton(floorRequestButtonID, "off", buttonFloor);
                floorRequestButtonsList.Add(floorRequestButton);
                buttonFloor--;
                floorRequestButtonID++;
            }
        }

        //Method to create buttons to request a floor
        public void makeFloorRequestButtons(int amountOfFloors) {
            int buttonFloor = 1;
            for (int i = 0; i < amountOfFloors; i++) {
                var floorRequestButton = new FloorRequestButton(floorRequestButtonID, "off", buttonFloor);
                floorRequestButtonsList.Add(floorRequestButton);
                buttonFloor++;
                floorRequestButtonID++;
            }
        }

        //Method to find the appropriate elevator within the appropriate column to serve user
        public void assignElevator(int requestedFloor, string direction) {
            Console.WriteLine($"A request for an elevator is made from the lobby for floor {requestedFloor}, going {direction}.");
            Column column = this.findBestColumn(requestedFloor); //returning column
            Console.WriteLine($"Column {column.ID} is the column that can handle this request.");
            Elevator elevator = column.findBestElevator(1, direction); //returning best elevator, 1 bescause this request is only made from lobby
            int stopFloor = elevator.floorRequestList[0];
            Console.WriteLine($"Elevator {elevator.ID} is the best elevator, so it is sent.");
            if (elevator.status == "moving") {
                elevator.moveElevator(stopFloor);
            }
            elevator.floorRequestList.Add(requestedFloor);
            elevator.sortFloorList();
            Console.WriteLine("Elevator is moving.");
            elevator.moveElevator(stopFloor);
            Console.WriteLine($"Elevator is {elevator.status}.");
            Door.doorController();
            if (elevator.floorRequestList.Count() == 0) {
                elevator.direction = null;
                elevator.status = "idle";
            }
            Console.WriteLine($"Elevator is {elevator.status}.");
        }

        //Method to find appropriate column to serve user
        public Column findBestColumn(int requestedFloor) {
            Column bestColumn = null;
            foreach (Column column in this.columnsList) {
                if (column.servedFloorsList.Contains(requestedFloor)) {
                    bestColumn = column;
                }
            }
            return bestColumn;
        }

    }

    //Defining column class
    public class Column {
        public int ID;
        public string status;
        public int amountOfElevators;
        public int[] servedFloorsList;
        public bool isBasement;
        public List<Elevator> elevatorsList;
        public List<CallButton> callButtonsList;

        public Column(int ID, string status, int amountOfElevators, int[] servedFloors, bool isBasement) {
            this.ID = ID;
            this.status = status;
            this.amountOfElevators = amountOfElevators;
            this.servedFloorsList = servedFloors;
            this.isBasement = isBasement;
            this.elevatorsList = new List<Elevator>();
            this.callButtonsList = new List<CallButton>();
        }

        //Method to create call buttons
        public void makeCallButtons(int floorsServed, int amountOfBasements, bool isBasement) {
            int callButtonID = 1;
            if (isBasement) {
                int buttonFloor = -1;
                for (int i = 0; i < amountOfBasements; i++) {
                    var callButton = new CallButton(callButtonID, "off", buttonFloor, "up");
                    callButtonsList.Add(callButton);
                    buttonFloor--;
                    callButtonID++;
                }
            } else {
                foreach (int floor in servedFloorsList) {
                    var callButton = new CallButton(callButtonID, "off", floor, "down");
                    callButtonsList.Add(callButton);
                    callButtonID++;
                }
            }
        }

        //Method to create elevators
        public void makeElevators(int[] servedFloorsList, int amountOfElevators) {
            int elevatorID = 1;
            for (int i = 0; i < amountOfElevators; i++) {
                var elevator = new Elevator(elevatorID, "idle", servedFloorsList, 1);
                elevatorsList.Add(elevator);
                elevatorID++;
            }
        }

        //When a user calls an elevator form a floor, not the lobby
        public void requestElevator(int userFloor, string direction) {
            Console.WriteLine($"A request for an elevator is made from floor {userFloor}, going {direction} to the lobby.");
            Elevator elevator = this.findBestElevator(userFloor, direction);
            Console.WriteLine($"Elevator {elevator.ID} is the best elevator, so it is sent.");
            elevator.floorRequestList.Add(1); //1 because elevator can only move to lobby from floors
            elevator.sortFloorList();
             Console.WriteLine("Elevator is moving.");
            elevator.moveElevator(userFloor);
            Console.WriteLine($"Elevator is {elevator.status}.");
            Door.doorController();
            if (elevator.floorRequestList.Count() == 0) {
                elevator.direction = null;
                elevator.status = "idle";
            }
            Console.WriteLine($"Elevator is {elevator.status}.");
        }

        //Find best elevator to send
        public Elevator findBestElevator(int floor, string direction) {
            int requestedFloor = floor;
            string requestedDirection = direction;
            var bestElevatorInfo = new BestElevatorInfo(null, 6, 1000000);

            if (requestedFloor == 1) {
                foreach (Elevator elevator in this.elevatorsList) {
                    //Elevator is at lobby with some requests, and about to leave but has not yet
                    if (1 == elevator.currentFloor && elevator.status == "stopped") {
                        this.checkBestElevator(1, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is at lobby with no requests
                    } else if (1 == elevator.currentFloor && elevator.status == "idle") {
                        this.checkBestElevator(2, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is lower than user and moving up. Shows user is requesting to go to basement, and elevator is moving to them.
                    } else if (1 > elevator.currentFloor && elevator.direction == "up") {
                        this.checkBestElevator(3, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is higher than user and moving down. Shows user is requesting to go to a floor, and elevator is moving to them.
                    } else if (1 < elevator.currentFloor && elevator.direction == "down") {
                        this.checkBestElevator(3, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is not at lobby floor, but has no requests
                    } else if (elevator.status == "idle") {
                        this.checkBestElevator(4, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is last resort
                    } else {
                        this.checkBestElevator(5, elevator, bestElevatorInfo, requestedFloor);
                    }
                }
            } else {
                foreach (Elevator elevator in this.elevatorsList) {
                    //Elevator is at floor going to lobby
                    if (requestedFloor == elevator.currentFloor && elevator.status == "stopped" && requestedDirection == elevator.direction) {
                        this.checkBestElevator(1, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is lower than user and moving through them to destination
                    } else if (requestedFloor > elevator.currentFloor && elevator.direction == "up" && requestedDirection == "up") {
                        this.checkBestElevator(2, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is higher than user and moving through them to destination
                    } else if (requestedFloor < elevator.currentFloor && elevator.direction == "down" && requestedDirection == "down") {
                        this.checkBestElevator(2, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is idle
                    } else if (elevator.status == "idle") {
                        this.checkBestElevator(3, elevator, bestElevatorInfo, requestedFloor);
                    //Elevator is last resort
                    } else {
                        this.checkBestElevator(4, elevator, bestElevatorInfo, requestedFloor);
                    }
                }
            }
            return bestElevatorInfo.bestElevator;

        }

        //Comparing elevator to previous best
        public BestElevatorInfo checkBestElevator(int scoreToCheck, Elevator newElevator, BestElevatorInfo bestElevatorInfo, int floor) {
            //If elevators situation is more favourable, set to best elevator
            if (scoreToCheck < bestElevatorInfo.bestScore) {
                bestElevatorInfo.bestScore = scoreToCheck;
                bestElevatorInfo.bestElevator = newElevator;
                bestElevatorInfo.referenceGap = Math.Abs(newElevator.currentFloor - floor);
            //If elevators are in a similar situation, set the closest one to the best elevator
            } else if (bestElevatorInfo.bestScore == scoreToCheck) {
                int gap = Math.Abs(newElevator.currentFloor - floor);
                if (bestElevatorInfo.referenceGap > gap) {
                    bestElevatorInfo.bestScore = scoreToCheck;
                    bestElevatorInfo.bestElevator = newElevator;
                    bestElevatorInfo.referenceGap = gap;
                }
            }
            return bestElevatorInfo;
        }

    }

    public class Elevator {
        public int ID;
        public string status;
        public int[] servedFloorsList;
        public int currentFloor;
        public string direction;
        public List<Door> door;
        public List<int> floorRequestList;

        public Elevator(int elevatorID, string status, int[] servedFloorsList, int currentFloor) {
            this.ID = elevatorID;
            this.status = status;
            this.servedFloorsList = servedFloorsList;
            this.currentFloor = currentFloor;
            this.direction = "";
            var door = new Door(elevatorID, "closed");
            this.floorRequestList = new List<int>();
        }

        //Moving elevator
        public void moveElevator(int stopFloor) {
            while (this.floorRequestList.Count() != 0) {
                int destination = this.floorRequestList[0];
                this.status = "moving";
                if (this.currentFloor < destination) {
                    this.direction = "up";
                    while (this.currentFloor < destination) {
                        if (this.currentFloor == stopFloor) {
                            this.status = "stopped";
                            Door.doorController();
                            this.currentFloor++;
                        } else {
                            this.currentFloor++;
                        }
                        if (this.currentFloor == 0) {
                            //Do nothing, so that moving from basement to/from 1 doesnt show 0
                        } else {
                            Console.WriteLine($"Elevator is at floor: {this.currentFloor}");
                        }
                    }
                } else if (this.currentFloor > destination) {
                    this.direction = "down";
                    while (this.currentFloor > destination) {
                        if (this.currentFloor == stopFloor) {
                            this.status = "stopped";
                            Door.doorController();
                            this.currentFloor--;
                        } else {
                            this.currentFloor--;
                        }
                        if (this.currentFloor == 0) {
                            //Do nothing, so that moving from basement to/from 1 doesnt show 0
                        } else {
                            Console.WriteLine($"Elevator is at floor: {this.currentFloor}");
                        }
                    }
                }
                this.status = "stopped";
                this.floorRequestList.RemoveAt(0);
            }
        }

        public void sortFloorList() {
            if (this.direction == "up") {
                this.floorRequestList.Sort((a, b) => a.CompareTo(b));
            } else {
                this.floorRequestList.Sort((a, b) => b.CompareTo(a));
            }
            
        }

    }

    //Defining best elevator info class
    public class BestElevatorInfo {
        public Elevator bestElevator;
        public int bestScore;
        public int referenceGap;

        public BestElevatorInfo(Elevator bestElevator, int bestScore, int referenceGap) {
            this.bestElevator = bestElevator;
            this.bestScore = bestScore;
            this.referenceGap = referenceGap;
        }
    }

    //Defining call button class
    public class CallButton {
        public int ID;
        public string status;
        public int floor;
        public string direction;

        public CallButton(int ID, string status, int floor, string direction) {
            this.ID = ID;
            this.status = status;
            this.floor = floor;
            this.direction = direction;
        }

    }

    //Defining floor request button class
    public class FloorRequestButton {
        public int ID;
        public string status;
        public int floor;

        public FloorRequestButton(int ID, string status, int floor) {
            this.ID = ID;
            this.status = status;
            this.floor = floor;
        }

    }

    //Defining door class
    public class Door {
        public int ID;
        public string status;

        public Door(int ID, string status) {
            this.ID = ID;
            this.status = status;
        }

        //Door operation controller
        public static void doorController() {
            bool overweight = false;
            bool obstruction = false;
            string status = "opened";
            Console.WriteLine($"Elevator doors are {status}.");
            Console.WriteLine("Waiting for occupant(s) to transition.");
            //Wait 5 seconds
            if (!overweight) {
                status = "closing";
                Console.WriteLine($"Elevator doors are {status}.");
                if (!obstruction) {
                    status = "closed";
                    Console.WriteLine($"Elevator doors are {status}.");
                } else {
                    //Wait for obstruction to clear
                    obstruction = false;
                    doorController();
                }
            } else {
                while (overweight) {
                    //Ring alarm and wait until not overweight
                    overweight = false;
                }
                doorController();
            }
        }

    }

    class Program {

        static void Main(string[] args) {

            var test = new Tests();
            

            //Uncomment to run scenario 1
            // test.scenario1();

            //Uncomment to run scenario 2
            // test.scenario2();

            //Uncomment to run scenario 3
            // test.scenario3();

            //Uncomment to run scenario 4
            // test.scenario4();
        }
    }

    public class Tests {
        
            public void scenario1() {
            
                var battery = new Battery(1, 4, "online", 60, 6, 5);

                battery.columnsList[1].elevatorsList[0].currentFloor = 20;
                battery.columnsList[1].elevatorsList[0].direction = "down";
                battery.columnsList[1].elevatorsList[0].status = "moving";
                battery.columnsList[1].elevatorsList[0].floorRequestList.Add(5);
                
                battery.columnsList[1].elevatorsList[1].currentFloor = 3;
                battery.columnsList[1].elevatorsList[1].direction = "up";
                battery.columnsList[1].elevatorsList[1].status = "moving";
                battery.columnsList[1].elevatorsList[1].floorRequestList.Add(15);
                
                battery.columnsList[1].elevatorsList[2].currentFloor = 13;
                battery.columnsList[1].elevatorsList[2].direction = "down";
                battery.columnsList[1].elevatorsList[2].status = "moving";
                battery.columnsList[1].elevatorsList[2].floorRequestList.Add(1);
                
                battery.columnsList[1].elevatorsList[3].currentFloor = 15;
                battery.columnsList[1].elevatorsList[3].direction = "down";
                battery.columnsList[1].elevatorsList[3].status = "moving";
                battery.columnsList[1].elevatorsList[3].floorRequestList.Add(2);
                
                battery.columnsList[1].elevatorsList[4].currentFloor = 6;
                battery.columnsList[1].elevatorsList[4].direction = "down";
                battery.columnsList[1].elevatorsList[4].status = "moving";
                battery.columnsList[1].elevatorsList[4].floorRequestList.Add(1);

                battery.assignElevator(20, "up");
            }

            public void scenario2() {
            
                var battery = new Battery(1, 4, "online", 60, 6, 5);

                battery.columnsList[2].elevatorsList[0].currentFloor = 1;
                battery.columnsList[2].elevatorsList[0].direction = "up";
                battery.columnsList[2].elevatorsList[0].status = "stopped";
                battery.columnsList[2].elevatorsList[0].floorRequestList.Add(21);
                
                battery.columnsList[2].elevatorsList[1].currentFloor = 23;
                battery.columnsList[2].elevatorsList[1].direction = "up";
                battery.columnsList[2].elevatorsList[1].status = "moving";
                battery.columnsList[2].elevatorsList[1].floorRequestList.Add(28);
                
                battery.columnsList[2].elevatorsList[2].currentFloor = 33;
                battery.columnsList[2].elevatorsList[2].direction = "down";
                battery.columnsList[2].elevatorsList[2].status = "moving";
                battery.columnsList[2].elevatorsList[2].floorRequestList.Add(1);
                
                battery.columnsList[2].elevatorsList[3].currentFloor = 40;
                battery.columnsList[2].elevatorsList[3].direction = "down";
                battery.columnsList[2].elevatorsList[3].status = "moving";
                battery.columnsList[2].elevatorsList[3].floorRequestList.Add(24);
                
                battery.columnsList[2].elevatorsList[4].currentFloor = 39;
                battery.columnsList[2].elevatorsList[4].direction = "down";
                battery.columnsList[2].elevatorsList[4].status = "moving";
                battery.columnsList[2].elevatorsList[4].floorRequestList.Add(1);

                battery.assignElevator(36, "up");
            }

            public void scenario3() {
            
                var battery = new Battery(1, 4, "online", 60, 6, 5);

                battery.columnsList[3].elevatorsList[0].currentFloor = 58;
                battery.columnsList[3].elevatorsList[0].direction = "down";
                battery.columnsList[3].elevatorsList[0].status = "moving";
                battery.columnsList[3].elevatorsList[0].floorRequestList.Add(1);
                
                battery.columnsList[3].elevatorsList[1].currentFloor = 50;
                battery.columnsList[3].elevatorsList[1].direction = "up";
                battery.columnsList[3].elevatorsList[1].status = "moving";
                battery.columnsList[3].elevatorsList[1].floorRequestList.Add(60);
            
                battery.columnsList[3].elevatorsList[2].currentFloor = 46;
                battery.columnsList[3].elevatorsList[2].direction = "up";
                battery.columnsList[3].elevatorsList[2].status = "moving";
                battery.columnsList[3].elevatorsList[2].floorRequestList.Add(58);
                
                battery.columnsList[3].elevatorsList[3].currentFloor = 1;
                battery.columnsList[3].elevatorsList[3].direction = "up";
                battery.columnsList[3].elevatorsList[3].status = "moving";
                battery.columnsList[3].elevatorsList[3].floorRequestList.Add(54);
                
                battery.columnsList[3].elevatorsList[4].currentFloor = 60;
                battery.columnsList[3].elevatorsList[4].direction = "down";
                battery.columnsList[3].elevatorsList[4].status = "moving";
                battery.columnsList[3].elevatorsList[4].floorRequestList.Add(1);

                battery.columnsList[3].requestElevator(54, "down");
            }

            public void scenario4() {
            
                var battery = new Battery(1, 4, "online", 60, 6, 5);

                battery.columnsList[0].elevatorsList[0].currentFloor = -4;
                
                battery.columnsList[0].elevatorsList[1].currentFloor = 1;
                
                battery.columnsList[0].elevatorsList[2].currentFloor = -3;
                battery.columnsList[0].elevatorsList[2].direction = "down";
                battery.columnsList[0].elevatorsList[2].status = "moving";
                battery.columnsList[0].elevatorsList[2].floorRequestList.Add(-5);
                
                battery.columnsList[0].elevatorsList[3].currentFloor = -6;
                battery.columnsList[0].elevatorsList[3].direction = "up";
                battery.columnsList[0].elevatorsList[3].status = "moving";
                battery.columnsList[0].elevatorsList[3].floorRequestList.Add(1);
                
                battery.columnsList[0].elevatorsList[4].currentFloor = -1;
                battery.columnsList[0].elevatorsList[4].direction = "down";
                battery.columnsList[0].elevatorsList[4].status = "moving";
                battery.columnsList[0].elevatorsList[4].floorRequestList.Add(-6);

                battery.columnsList[0].requestElevator(-3, "up");
            }
    }

}
