package com.example.myapplication.Item;

//This is the class for the information between 2 stops
public class RoadInfoItem {
    //the line
    private String line;
    //the first stop
    private String departure;
    //the second stop
    private String arrival;
    //the time you have to wait until a bus come to the first stop
    private int timeLeft;

    public RoadInfoItem(String line,String departure, String arrival, int timeLeft) {
        this.line = line;
        this.departure = departure;
        this.arrival = arrival;
        this.timeLeft = timeLeft;
    }

    public String getDeparture() {
        return departure;
    }

    public void setDeparture(String departure) {
        this.departure = departure;
    }

    public String getArrival() {
        return arrival;
    }

    public void setArrival(String arrival) {
        this.arrival = arrival;
    }

    public int getTimeLeft() {
        return timeLeft;
    }

    public void setTimeLeft(int timeLeft) {
        this.timeLeft = timeLeft;
    }

    public String getLine() {
        return line;
    }

    public void setLine(String line) {
        this.line = line;
    }
}
