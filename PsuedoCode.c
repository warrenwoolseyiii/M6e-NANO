
// scans for a tag for duration amount of milliseconds, returns true if a tag is found
bool scanForTag(uint32_t durationInMillis, tag_data_t &tag)
{
   uint32_t startTimeInMillis = millis();

   // ensure you have the correct read settings etc.
   // start a continuos read on the RFID tag reader

   while ((millis() - startTimeInMillis) < durationInMillis)
   {
      // check the reader for a hit
      // if we have a hit, make sure its a full hit
      {
         // stop reading
         // get the data
         // tag.rssi = nano.getTagRSSI();
         // tag.freq = nano.getTagFreq();
         // tag.timeStamp = nano.getTagTimeStamp(); <-- need to think about how i want to time stamp these things
         // tag.epc = nano.getEPC();  
         return TRUE;
      }
   }

   return FALSE;
}

// checks a tag epc for mob armor markings, returns true if its one of ours
bool filterMobArmorEPC(tag_data_t &tag)
{
   // compare the EPC against known mob armor stuff
}

// this is the nasty one, need to run the current tag against all the tags stored in a massive non volatile 
// memory chip. Probably external EEPROM or somthing. If the tag exists, update the latest time stamp. Otherwise
// add it to the list
void runTagAgainstExistingTags(tag_data_t &tag)
{
   tag_data_t local;
   // while we have not reached the end of existing tags
   {
      // read the next value from the table and stor it in local
      local = readTagValueFromMemory(someAddress);
      
      // compare the tag values
      if (compareTags(tag, local))
      {
         // tags are duplicate
         writeTagValueToMemory(tag, someAddress);
         return;
      }
   }

   // if we parse through the whole list and haven't found anything, append the list
   writeTagValueToMemory(tag, someAddress);
}

// this is also a nasty one, parses through each tag in the list and looks for timestamps that are 
// older than tagRemovedTimeOut.
void markRemovedTags(uint32_t tagRemovedTimeOut)
{
   tag_data_t local;
   uint32_t totalTagsInMemory, numTagsRemoved;

   totalTagsInMemory = 0;
   numTagsRemoved = 0;

   // while we have not reached the end of existing tags
   {
      // read the next value from the table and store it in local
      local = readTagValueFromMemory(someAddress);
      totalTagsInMemory++;
      // check the time stamp on the tag and compare it to tagRemovedTimeOut
      // if the time stampe is too old, mark it as removed
      {
         markTagAsRemoved(someAddress);
         numTagsRemoved++;
      }
   }

   // get a final tag count
   gTotalNumTags = (totalTagsInMemory - numTagsRemoved);
}

// pretty easy one, just delete all the removed tags after we transmit them
void deletRemovedTags()
{
   tag_data_t local;
   // while we have not reached the end of existing tags
   {

   }
}

// run it all!
void main()
{
   tag_data_t tag;

   // scan for existing tags and update the table
   if (scanForTag(1000, tag))
   {
      if (filterMobArmorEPC(tag))
         runTagAgainstExistingTags(tag);
   }
   
   // check the system time, is it time to parse out removed tags?
   {
      markRemovedTags(gTagRemovedTimeOut);
      // transmit the data! this will be a huge undertaking...
      deleteRemovedTags();
   }
}