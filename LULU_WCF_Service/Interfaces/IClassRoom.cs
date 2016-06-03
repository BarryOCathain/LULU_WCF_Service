﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LULU_WCF_Service.Interfaces
{
    [ServiceContract]
    interface IClassRoom
    {
        [OperationContract]
        bool AddClassRoom(string classroomString);

        [OperationContract]
        bool DeleteClassRoom(string classroomString);

        [OperationContract]
        bool UpdateClassRoom(string classRoomString);

        [OperationContract]
        string GetAllClassRooms();

        [OperationContract]
        string GetAllClassRoomsByCampus(string campusString);
    }
}
