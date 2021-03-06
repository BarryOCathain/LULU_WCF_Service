﻿using LULU_WCF_Service.Interfaces;
using System;
using System.Linq;
using LULU.Model;
using LULU.Model.Common;
using System.Reflection;
using log4net;
using System.Collections.Generic;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace LULU_WCF_Service
{
    public class LULU_Service : IStudent, ICampus, IClass, IClassRoom, ICourse, IUser
    {
        #region Private Members
        // log4net object creation. Uses System.Reflection to get the executing method and add details to logs.
        // Configuration in App.Config.
        private static readonly ILog logs = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private LULU_ModelContainer context;
        #endregion

        #region Constructor
        public LULU_Service()
        {
            context = new LULU_ModelContainer();

            // Stops the cration of proxy objects at runtime which causes issues with serialization
            context.Configuration.ProxyCreationEnabled = false;

            // Stops Lazy Loading which will load all related entities the first time that an object is accessed.
            // This could be a problem where there are thousands of related entiies.
            // For example, if we queried for a specific ClassRoom, then with Lazy Loading, the first time that we access
            // this ClassRoom, all related Classes would be loaded into memory.
            context.Configuration.LazyLoadingEnabled = false;
        }
        #endregion

        #region IStudent Implementation
        public bool CreateStudent(string studentNumber, string firstName, string surname, string email, string password)
        {
            try
            {
                context.Users.Add(new Student
                {
                    StudentNumber = studentNumber,
                    FirstName = firstName,
                    Surname = surname,
                    Email = email,
                    Password = password
                });
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred creating the new student object.", ex);
            }
            return false;
        }

        public bool DeleteStudent(string studentNumber)
        {
            try
            {
                Student st = context.Users.OfType<Student>()
                .Where(s => s.StudentNumber == studentNumber).FirstOrDefault();

                context.Users.Remove(st);

                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred deleting the student with Student Number: " + studentNumber, ex);
            }
            return false;
        }

        public bool UpdateStudent(string studentNumber, string firstName, string surname, string email, string password)
        {
            try
            {
                Student st = context.Users.OfType<Student>()
                        .Where(s => s.StudentNumber == studentNumber).FirstOrDefault();

                st.FirstName = firstName;
                st.Surname = surname;
                st.Email = email;
                st.Password = password;

                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred updating the student with Student Number: " + studentNumber, ex);
            }
            return false;
        }

        public string GetAllStudents()
        {
            try
            {
                return Serializers<Student>.SerializeList(context.Users.OfType<Student>().ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving all students", ex);
            }
            return null;
        }

        public string GetStudentByUserID(int userID)
        {
            try
            {
                return Serializers<Student>.Serialize(context.Users.OfType<Student>().Where(s => s.UserID == userID).FirstOrDefault());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the student with the UserID: " + userID, ex);
            }
            return null;
        }
        
        public string GetStudentByStudentNumber(string studentNumber)
        {
            try
            {
                return Serializers<Student>.Serialize(context.Users.OfType<Student>().Where(s => s.StudentNumber.Equals(studentNumber)).FirstOrDefault());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the student with Student Number: " + studentNumber, ex);
            }
            return null;
        }

        public string SearchStudentsByFirstName(string firstName)
        {
            try
            {
                return Serializers<Student>.SerializeList(context.Users.OfType<Student>().Where(s => s.FirstName.Equals(firstName)).ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving students by first name", ex);
            }
            return null;
        }

        public string SearchStudentsBySurname(string surname)
        {
            try
            {
                return Serializers<Student>.SerializeList(context.Users.OfType<Student>().Where(s => s.Surname.Equals(surname)).ToList());
            }
            catch (Exception ex)
            {

                logs.Error("An error occurred retrieving students by surname", ex);
            }
            return null;
        }

        public string SearchStudentByStudentNumber(string studentNumber)
        {
            try
            {
                return Serializers<Student>.Serialize(context.Users.OfType<Student>()
                        .Where(s => s.StudentNumber == studentNumber).FirstOrDefault());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving a student with Student Number: " + studentNumber, ex);
            }
            return null;
        }

        public bool LoginStudent(string studentNumber, string password)
        {
            try
            {
                var student = context.Users.OfType<Student>()
                        .Where(s => s.StudentNumber.Equals(studentNumber) && s.Password.Equals(password))
                        .FirstOrDefault();
                return student != null;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred logging in the Student with StudentNumber " + studentNumber, ex);
            }
            return false;
        }

        public bool StudentAttendedClass(string studentNumber, int classID, string loginString)
        {
            try
            {
                Student student = context.Users.OfType<Student>().Where(s => s.StudentNumber.Equals(studentNumber)).FirstOrDefault();
                Class _class = context.Classes.Where(c => c.ClassID == classID).FirstOrDefault();

                Login login = Serializers<GPS_Login>.Deserialize(loginString);

                if (login == null)
                    login = Serializers<Staff_Login>.Deserialize(loginString);

                if (student != null && _class != null && login != null)
                {
                    context.AtttendedClasses.Add(new AtttendedClass()
                    {
                        Class = _class,
                        Student = student,
                        GPS_Login = login is GPS_Login ? (GPS_Login)login : null,
                        Staff_Login = login is Staff_Login ? (Staff_Login)login : null
                    });

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logs.Error(string.Format("An error occurred adding Student {0} to class {1}", studentNumber, classID), ex);
            }
            return false;
        }
        #endregion

        #region ICampus Implementation
        public bool AddCampus(string name)
        {
            try
            {
                Campus cs = context.Campus.Where(c => c.Name.Equals(name)).FirstOrDefault();

                if (cs == null)
                {
                    context.Campus.Add(new Campus
                    {
                        Name = name
                    });

                    context.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred Adding the new Campus", ex);
            }
            return false;
        }

        public bool DeleteCampus(int campusID)
        {
            try
            {
                Campus cs = context.Campus.Where(c => c.CampusID == campusID).FirstOrDefault();

                if (cs != null)
                {
                    context.Campus.Remove(cs);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred deleting the Campus with CampusID: " + campusID, ex);
            }
            return false;
        }

        public string GetAllCampuses()
        {
            try
            {
                return Serializers<Campus>.SerializeList(context.Campus.ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving all Campuses", ex);
            }
            return null;
        }

        public string GetCampusByClassroom(int classroomID)
        {
            try
            {
                return Serializers<Campus>.Serialize(context.ClassRooms1.Where(c => c.ClassRoomID == classroomID).FirstOrDefault().Campus);
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the Campus with ClassroomID: " + classroomID, ex);
            }
            return null;
        }
        #endregion

        #region IClass Implementation
        public bool AddClass(string newClass)
        {
            try
            {
                Class c = null;

                c = Serializers<Class>.Deserialize(newClass);

                if (c != null)
                {
                    context.Classes.Add(c);
                    context.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred adding the new class", ex);
            }
            return false;
        }

        public bool DeleteClass(int classID)
        {
            try
            {
                Class cl = context.Classes.Where(c => c.ClassID == classID).FirstOrDefault();

                if (cl != null)
                {
                    context.Classes.Remove(cl);
                    context.SaveChanges();
                    return false;
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred deleting the Class with ClassID: " + classID, ex);
            }
            return false;
        }

        public bool UpdateClass(string updatedClass)
        {
            try
            {
                Class c = null;

                c = Serializers<Class>.Deserialize(updatedClass.Trim());

                if (c != null)
                {
                    Class original = context.Classes.Where(cl => cl.ClassID == c.ClassID).FirstOrDefault();

                    if (original != null)
                    {
                        original.Name = c.Name;
                        original.ClassDate = c.ClassDate;
                        original.Compulsory = c.Compulsory;
                        original.StartTime = c.StartTime;
                        original.EndTime = c.EndTime;
                        original.Course = c.Course;
                        original.ClassRoom = c.ClassRoom;

                        context.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred updating the Class", ex);
            }
            return false;
        }

        public string GetAllClasses()
        {
            try
            {
                return Serializers<Class>.SerializeList(context.Classes
                    .Include("Course")
                    .Include("ClassRoom")
                    .Include("ClassRoom.Campus")
                    .ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving all Classes", ex);
            }
            return null;
        }

        public string GetAllClassesByCourse(int courseID)
        {
            try
            {
                return Serializers<Class>.SerializeList(context.Classes.Where(c => c.Course.CourseID == courseID).ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving all Classes for CourseID: " + courseID, ex);
            }
            return null;
        }

        public string GetAllClassesByDate(DateTime classDate)
        {
            try
            {
                return Serializers<Class>.SerializeList(context.Classes.Where(c => c.ClassDate == classDate).ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving Classes on the date: " + classDate, ex);
            }
            return null;
        }

        public string GetAllClassesByClassroom(int classroomID)
        {
            try
            {
                return Serializers<Class>.SerializeList(context.Classes.Where(c => c.ClassRoom.ClassRoomID == classroomID).ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving Classes for ClassroomID: " + classroomID, ex);
            }
            return null;
        }

        public string GetClassesByName(string name)
        {
            try
            {
                return Serializers<Class>.SerializeList(context.Classes.Where(c => c.Name.Contains(name)).ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving Classes with the Name: " + name, ex);
            }
            return null;
        }

        public string GetClassByID(int classID)
        {
            try
            {
                return Serializers<Class>.Serialize(context.Classes.Where(c => c.ClassID == classID).FirstOrDefault());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the Class with ClassID: " + classID, ex);
            }
            return null;
        }

        public string GetClassesByStudentNumberAndDateRange(string studentNumber, DateTime startDate, DateTime endDate, bool includeAttendedClasees)
        {
            try
            {
                Student student = context.Users.OfType<Student>().Where(s => s.StudentNumber.Equals(studentNumber)).FirstOrDefault();
                if (student != null)
                {
                    var courses = context.Users.AsNoTracking().OfType<Student>().Select(s => s.Courses).FirstOrDefault();
                    Course course = courses.First();

                    var result = context.Classes.Include("AtttendedClasses").Where(c => c.Course.CourseID == course.CourseID && c.ClassDate >= startDate
                        && c.ClassDate <= endDate).ToList();

                    if (includeAttendedClasees)
                        return Serializers<Class>.SerializeList(result);

                    var results = new List<Class>();

                    foreach (var _class in result)
                    {
                        if (!_class.AtttendedClasses.Any())
                            results.Add(_class);
                    }

                    return Serializers<Class>.SerializeList(results);
                }
            }
            catch (Exception ex)
            {
                logs.Error(string.Format("An error occurred retrieving the attended classes for Student {0}, between {1} and {2}", studentNumber,
                    startDate.ToShortDateString(), endDate.ToShortDateString()), ex);
            }
            return null;
        }

        public string GetAttendedClassesByStudentNumberAndDateRange(string studentNumber, DateTime startDate, DateTime endDate)
        {
            try
            {
                Student student = context.Users.OfType<Student>().Where(s => s.StudentNumber.Equals(studentNumber)).FirstOrDefault();
                if (student != null)
                {
                    var courses = context.Users.AsNoTracking().OfType<Student>().Select(s => s.Courses).FirstOrDefault();
                    Course course = courses.First();

                    var result = context.Classes.Include("AtttendedClasses").Where(c => c.Course.CourseID == course.CourseID && c.ClassDate >= startDate
                        && c.ClassDate < endDate).ToList();

                    var results = new List<Class>();

                    foreach (var _class in result)
                    {
                        if (_class.AtttendedClasses.Any())
                            results.Add(new Class()
                            {
                                ClassID = _class.ClassID,
                                Name = _class.Name,
                                ClassDate = _class.ClassDate,
                                Compulsory = _class.Compulsory,
                                StartTime = _class.StartTime,
                                EndTime = _class.EndTime
                            });

                    }

                    return Serializers<Class>.SerializeList(results);
                }
            }
            catch (Exception ex)
            {
                logs.Error(string.Format("An error occurred retrieving the attended classes for Student {0}, between {1} and {2}", studentNumber,
                    startDate.ToShortDateString(), endDate.ToShortDateString()), ex);
            }
            return null;
        }

        public string GetMissedClassesByStudentNumberAndDateRange(string studentNumber, DateTime startDate, DateTime endDate)
        {
            try
            {
                Student student = context.Users.OfType<Student>().Where(s => s.StudentNumber.Equals(studentNumber)).FirstOrDefault();
                if (student != null)
                {
                    var courses = context.Users.AsNoTracking().OfType<Student>().Select(s => s.Courses).FirstOrDefault();
                    Course course = courses.First();

                    var result = context.Classes.Include("AtttendedClasses").Where(c => c.Course.CourseID == course.CourseID && c.ClassDate >= startDate
                        && c.ClassDate < endDate).ToList();

                    var results = new List<Class>();

                    foreach (var _class in result)
                    {
                        if (_class.AtttendedClasses.Any())
                            results.Add(new Class()
                            {
                                ClassID = _class.ClassID,
                                Name = _class.Name,
                                ClassDate = _class.ClassDate,
                                Compulsory = _class.Compulsory,
                                StartTime = _class.StartTime,
                                EndTime = _class.EndTime
                            });

                    }

                    return Serializers<Class>.SerializeList(results);
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the missed classes for Student " + studentNumber, ex);
            }
            return null;
        }
        #endregion

        #region IClassRoom Implementation
        public bool AddClassRoom(string name, decimal longitude, decimal latitude, int campusID)
        {
            try
            {
                Campus campus = context.Campus.Where(c => c.CampusID == campusID).FirstOrDefault();

                if (campus != null)
                {
                    context.ClassRooms1.Add(new ClassRoom()
                    {
                        Name = name,
                        Longitude = longitude,
                        Latitude = latitude,
                        Campus = campus
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred adding the new Classroom.", ex);
            }
            return false;
        }

        public bool DeleteClassRoom(int classroomID)
        {
            ClassRoom cl = context.ClassRooms1.Where(c => c.ClassRoomID == classroomID).FirstOrDefault();

            if (cl != null)
            {
                try
                {
                    context.ClassRooms1.Remove(cl);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logs.Error("An error occurred deleting the Classroom with ClassroomID: " + classroomID, ex);
                }
            }
            return false;
        }

        public bool UpdateClassRoom(string classRoomString)
        {
            ClassRoom cl = Serializers<ClassRoom>.Deserialize(classRoomString);

            if (cl != null)
            {
                if (context.ClassRooms1.Any(c => c.Equals(cl)))
                {
                    try
                    {
                        ClassRoom classroomToUpdate = context.ClassRooms1.Where(c => c.Equals(cl)).FirstOrDefault();

                        classroomToUpdate.Name = cl.Name;
                        classroomToUpdate.Longitude = cl.Longitude;
                        classroomToUpdate.Latitude = cl.Latitude;
                        classroomToUpdate.Campus = cl.Campus;

                        context.SaveChanges();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        logs.Error("An error occurred updating the Classroom with ClassroomID: " + cl.ClassRoomID, ex);
                    }
                }
            }
            return false;
        }

        public string GetAllClassRooms()
        {
            try
            {
                return Serializers<ClassRoom>.SerializeList(context.ClassRooms1.ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving all Classrooms.", ex);
            }
            return null;
        }

        public string GetAllClassRoomsByCampus(int campusID)
        {
            try
            {
                return Serializers<ClassRoom>.SerializeList(context.ClassRooms1.Where(cl => cl.Campus.CampusID == campusID).ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the Classrooms for CampusID: " + campusID, ex);
            }
            return null;
        }

        public string GetClassRoomByID(int classroomID)
        {
            try
            {
                return Serializers<ClassRoom>.Serialize(context.ClassRooms1.Where(c => c.ClassRoomID == classroomID).First());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the Classroom with ID: " + classroomID, ex);
            }
            return null;
        }

        public string GetClassRoomByClassID(int classID)
        {
            return Serializers<ClassRoom>.Serialize(context.Classes.Where(c => c.ClassID == classID).Select(cl => cl.ClassRoom).FirstOrDefault());
        }
        #endregion

        #region ICourse Implementation
        public bool AddCourse(string courseCode, string name)
        {
            try
            {
                context.Courses1.Add(new Course()
                {
                    CourseCode = courseCode,
                    Name = name
                });
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred adding the new Course.", ex);
            }
            return false;
        }

        public bool DeleteCourse(int courseID)
        {
            Course course = context.Courses1.Where(c => c.CourseID == courseID).FirstOrDefault();

            try
            {
                if (course != null)
                {
                    context.Courses1.Remove(course);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred deleting the Course with CourseID: " + courseID, ex);
            }
            return false;
        }

        public bool UpdateCourse(string courseString)
        {
            Course course = Serializers<Course>.Deserialize(courseString);

            try
            {
                if (course != null)
                {
                    if (context.Courses1.Any(c => c.CourseID == course.CourseID))
                    {
                        Course courseToUpdate = context.Courses1.Where(c => c.CourseID == course.CourseID).FirstOrDefault();

                        courseToUpdate.CourseCode = course.CourseCode;
                        courseToUpdate.Name = course.Name;

                        context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred updating the Course with CourseID: " + course.CourseID, ex);
            }
            return false;
        }

        public string GetAllCourses()
        {
            try
            {
                return Serializers<Course>.SerializeList(context.Courses1.ToList());
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving all Courses.", ex);
            }
            return null;
        }

        public string GetCourseByID(int courseID)
        {
            var course = context.Courses1.Where(c => c.CourseID == courseID).FirstOrDefault();

            if (course != null)
                return Serializers<Course>.Serialize(course);
            return null;
        }

        public string GetCourseByCourseCode(string courseCode)
        {
            var course = context.Courses1.Where(c => c.CourseCode.Equals(courseCode)).FirstOrDefault();

            if (course != null)
                return Serializers<Course>.Serialize(course);
            return null;
        }
        #endregion

        #region IUser Implementation
        public bool AddLecturer(string title, string staffNumber, string firstName, string surname, string email, string password, bool isSysAdmin)
        {
            try
            {
                context.Users.Add(new Lecturer()
                {
                    Title = title,
                    StaffNumber = staffNumber,
                    FirstName = firstName,
                    Surname = surname,
                    Email = email,
                    Password = password,
                    IsSysAdmin = isSysAdmin
                });
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred saving the new Lecturer.", ex);
            }
            return false;
        }

        public bool AddStaffUser(string staffNumber, string firstName, string surname, string email, string password, bool isSysAdmin)
        {
            try
            {
                context.Users.Add(new Staff_User()
                {
                    StaffNumber = staffNumber,
                    FirstName = firstName,
                    Surname = surname,
                    Email = email,
                    Password = password,
                    IsSysAdmin = isSysAdmin
                });
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred saving the new Staff User.", ex);
            }
            return false;
        }

        public bool AddStudent(string studentNumber, string firstName, string surname, string email, string password)
        {
            try
            {
                context.Users.Add(new Student()
                {
                    StudentNumber = studentNumber,
                    FirstName = firstName,
                    Surname = surname,
                    Email = email,
                    Password = password
                });
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred saving the new Student.", ex);
            }
            return false;
        }

        public bool DeleteUser(int userID)
        {
            User user = context.Users.Where(u => u.UserID == userID).FirstOrDefault();

            if (user != null)
            {
                try
                {
                    context.Users.Remove(user);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logs.Error("An error occurred deleting the User with UserID: " + userID, ex);
                }
            }
            return false;
        }

        public bool UpdateUser(string userString)
        {
            User user = Serializers<User>.Deserialize(userString);

            if (user != null)
            {
                try
                {
                    User userToUpdate = context.Users.Where(u => u.UserID == user.UserID).FirstOrDefault();

                    if (userToUpdate != null)
                    {
                        userToUpdate.FirstName = user.FirstName;
                        userToUpdate.Surname = user.Surname;
                        userToUpdate.Email = user.Email;
                        userToUpdate.Password = user.Password;

                        if (user is Student)
                            ((Student)userToUpdate).StudentNumber = ((Student)user).StudentNumber;

                        if (user is Staff_User)
                        {
                            ((Staff_User)userToUpdate).StaffNumber = ((Staff_User)user).StaffNumber;
                            ((Staff_User)userToUpdate).IsSysAdmin = ((Staff_User)user).IsSysAdmin;
                        }

                        if (user is Lecturer)
                        {
                            ((Lecturer)userToUpdate).StaffNumber = ((Lecturer)user).StaffNumber;
                            ((Lecturer)userToUpdate).IsSysAdmin = ((Lecturer)user).IsSysAdmin;
                            ((Lecturer)userToUpdate).Title = ((Lecturer)user).Title;
                        }
                    }
                    context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logs.Error("An error occurred updating the User with UserID: " + user.UserID, ex);
                }
            }
            return false;
        }

        public string GetAllUsersOfType(string typeString)
        {
            try
            {
                Assembly model = typeof(User).Assembly;
                Type userType = model.GetType(typeString);

                if (userType != null)
                {
                    if (userType == typeof(Student))
                        return Serializers<Student>.SerializeList(context.Users.OfType<Student>().ToList());
                    else if (userType == typeof(Staff_User))
                        return Serializers<Staff_User>.SerializeList(context.Users.OfType<Staff_User>().ToList());
                    else if (userType == typeof(Lecturer))
                        Serializers<Lecturer>.SerializeList(context.Users.OfType<Lecturer>().ToList());
                }
            }
            catch (Exception ex)
            {
                logs.Error("An error occurred retrieving the Users of type " + typeString, ex);
            }
            return null;
        }

        public string LoginStaffUser(string staffNumber, string password)
        {
            var user = context.Users.OfType<Staff_User>().Where(s => s.StaffNumber.Equals(staffNumber) && s.Password.Equals(password)).FirstOrDefault();

            if (user != null)
                return Serializers<Staff_User>.Serialize(user);
            return null;
        }
        #endregion
    }
}
