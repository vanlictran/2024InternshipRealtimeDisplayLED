package com.example.myapplication.Item;

import java.util.List;
//This is the class for the alert for the bus line
public class TrafficInfoItem {
    //title of the alert
    private String title;
    //description of the alert
    private String description;
    //List of the impacted lines
    private List<String> numbers;

    public TrafficInfoItem(String title,String description, List<String> numbers) {
        this.title = title;
        this.description = description;
        this.numbers = numbers;
    }

    public String getDescription() {
        return description;
    }

    public String getTitle() {
        return title;
    }

    public List<String> getNumbers() {
        return numbers;
    }

}
