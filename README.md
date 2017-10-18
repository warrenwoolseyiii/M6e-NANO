# Mob-Armor-RFID
Mob Armor RFID inventory tracking project

# Design Decisions
* **(1)** For the RFID tag reader, we need to figure out what kind of external memory we want to use.
* **(2)** Need to figure out what kind of data we want to transfer, I am thinking the following: Inventory Present (EPCs), Inventory Removed (EPCs), total inventory stocked, total inventory removed 
* **(3)** we have to figure out how we want to store all the tag data on board the embedded system. I will have to research large listing schemes that can be employed

# TODO
## Tag Data Storage
As listed above a tag data storage scheme will be needed. We will need to store the tag data and easily parse through the list for comparison, update, and removal of certain tags. The storage scheme must be non volatile in the event of a loss of power. Read write speed will be crucial here since we could be dealing with a lot of data? maybe...
## Tag Data Parsing Algorithms
We can actually get started on this right away. Assuming we have some magical memory I can simulate the memory on the computer and write the algorithms to parse, compare, over write, and store tag data. 
## Figure out exactly how much data we want to send
Simple, how much data do we want to send. We should store as amuch as possible, we can have send settings to control what we do and do not send.