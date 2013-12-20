alter table dbo.Project alter column Caption nvarchar(255)
go

alter VIEW dbo.ActualProjectView 
AS
	select q.ProjectID,
		   q.ProjectPriority,
		   q.ProjectDeadlineDate,
		   case when q.ProjectState = 3 and q.ProjectProgress = 100 then N'Выполнено'
	            when l.IsDeadLineSet = 0 then N'Срок не задан'
	            when l.HoursLeft < 0 then N'Просрочено'
	            when q.ProjectDeadlineDate >= l.Today and q.ProjectDeadlineDate < l.Tomorrow then N'Сегодня'
	            when q.ProjectDeadlineDate >= l.Tomorrow and q.ProjectDeadlineDate < dateadd(day, 1, l.Tomorrow) then N'Завтра'
	            when datepart(week, q.ProjectDeadlineDate) = datepart(week, getdate()) then N'На этой неделе'
	            when datepart(week, q.ProjectDeadlineDate) - datepart(week, getdate()) = 1 then N'На следующей неделе'
	            when datepart(month, q.ProjectDeadlineDate) = datepart(month, getdate()) then N'В этом месяце'
			    when datepart(month, q.ProjectDeadlineDate) - datepart(month, getdate()) = 1 then N'В следующем месяце'
	            when datepart(year, q.ProjectDeadlineDate) = datepart(year, getdate()) then N'В этом году'
	            when datepart(year, q.ProjectDeadlineDate) - datepart(year, getdate()) > 0 then N'Через несколько лет'
	       end as DeadLineaption,
		   q.ProjectCaption,
		   q.ProjectTypeName,
	       q.ContactName,
	       q.ContactPhone,
	       q.CustomerName,
	       q.ProjectProgress,
	       q.ResponsiblePerson,
	       q.LastStageName,
           q.ProjectPrice,
           q.ProjectPaid,
	       cast(case when q.IsInWork = 1 and q.IsProductionCompleted = 0 and q.ProjectState = 1 and q.ProjectPriority in (4,5) then 1 else 0 end as bit) as IsHighPriority,
	       cast(case when q.IsInWork = 1 and q.IsProductionCompleted = 0 and q.ProjectState = 1 and q.ProjectDeadlineDate >= l.Today and q.ProjectDeadlineDate < l.Tomorrow then 1 else 0 end as bit) as IsToday,
	       cast(case when q.IsInWork = 1 and q.IsProductionCompleted = 0 and q.ProjectState = 1 then 1 else 0 end as bit) as IsCurrent,
	       cast(case when q.IsInWork = 1 and q.IsProductionCompleted = 0 and q.ProjectState = 1 and l.HoursLeft < 0 then 1 else 0 end as bit) as IsOverstay,
	       cast(case when q.IsInWork = 0 and q.ProjectState = 1 and q.ProjectState = 1 then 1 else 0 end as bit) as IsPrepare,
	       cast(case when q.ProjectState = 3 then 1 else 0 end as bit) as IsArchive,
           cast(q.IsProductionCompleted as bit) as IsProductionCompleted
      from (select p.ID as ProjectID,
				   p.Caption as ProjectCaption,
				   dt.Value as ProjectTypeName,
				   p.Priority as ProjectPriority,
				   c.Name as ContactName,
				   c.PhoneNumber as ContactPhone,
				   cs.Name as CustomerName,
				   p.Progress as ProjectProgress,
				   p.ResponsiblePerson,
				   (select top 1 max(sp.DateTImeValue) /*последний заданный срок сдачи*/
					  from StageParameter sp
					       inner join ProjectStage ps
					    on (sp.ProjectStageID = ps.ID)
					 where ps.ProjectID = p.ID
					   and sp.ParameterID = 11 /*срок сдачи*/
                       and ps.StageState = 2
					   and ps.RowState = 1) as ProjectDeadlineDate,
				   (select top 1
			               ps.Name
			          from ProjectStage ps
			         where ps.ProjectID = p.ID
			           and ps.RowState = 1
			         order by ps.SortIndex desc) as LastStageName,
				   p.IsInWork,
				   p.RowState as ProjectState,
                   case when p.IsInWork = 1
                         and p.RowState = 1
                         and exists(select 1
                                      from ProjectStage ps
                                     where ps.ProjectID = p.ID
                                       and ps.ResponsibleRole = 4 /*художник-оформитель*/
                                       and ps.RowState = 1)
                        then case when exists(select 1
                                                from ProjectStage ps
                                               where ps.ProjectID = p.ID
                                                 and ps.ResponsibleRole = 4 /*художник-оформитель*/
                                                 and ps.StageState = 1 /*в работе*/
                                                 and ps.RowState = 1)
                                  then 0
                                  else 1
                             end
                        else 0 
                   end as IsProductionCompleted,
                   isnull((select top 1
                                  sp.NumberValue
                             from ProjectStage ps
                                  inner join StageParameter sp
                               on (sp.ProjectStageID = ps.ID)
                            where ps.ProjectID = p.ID
                              and ps.StageState <> 3
                              and ps.RowState = 1
                              and sp.ParameterID = 21), cast(0 as decimal(13,2))) as ProjectPrice,
                   isnull((select sum(sp.NumberValue)
                             from ProjectStage ps
                                  inner join StageParameter sp
                               on (sp.ProjectStageID = ps.ID)
                            where ps.ProjectID = p.ID
                              and ps.StageState <> 3
                              and ps.RowState = 1
                              and sp.ParameterID = 17), cast(0 as decimal(13,2))) as ProjectPaid
			  from Project p
				   inner join Dictionary dt
				on (p.Type = dt.ID and dt.DictionaryNumber = 1)
				   inner join Contact c
				on (c.ID = p.ContactID)
				   inner join Customer cs
				on (cs.ID = c.CustomerID)
			 where p.RowState in (1,3)) q
	       cross apply (select datediff(hour, getdate(), q.ProjectDeadlineDate) as HoursLeft,
						       case when q.ProjectDeadlineDate is not null
						            then 1
						            else 0
						       end as IsDeadLineSet,
						       dateadd(day, datediff(day, 0, getdate()), 0) as Today,
						       dateadd(day, datediff(day, 0, getdate()), 1) as Tomorrow) as l
	 where 1 = 1
GO

CREATE VIEW dbo.ProjectPivotView 
AS
select p.ID as ProjectID,
       cs.Name as CustomerName, 
       p.Caption as ProjectCaption,
       p.Note as ProjectNote,
       p.[Priority] as ProjectPriority,
       p.[Type] as ProjectType,
       case when p.Progress = 100 then 1
            when p.Progress < 100 and p.IsInWork = 1 then 2
            when p.Progress < 100 and p.IsInWork = 0 then 3
       end as ProjectState,
       rtrim(ltrim(p.ResponsiblePerson)) as ProjectResponsiblePerson,
       (select max(sp.DateTimeValue) 
          from ProjectStage ps
               inner join StageParameter sp
            on ps.ID = sp.ProjectStageID
         where ps.ProjectID = p.ID
           and sp.ParameterID = 11) as DeadLine,
       isnull((select top 1
               sp.NumberValue
          from ProjectStage ps
               inner join StageParameter sp
            on ps.ID = sp.ProjectStageID
         where ps.ProjectID = p.ID
           and sp.ParameterID = 21), 0) as ProjectPrice
  from Project p
       inner join Contact c
    on c.ID = p.ContactID
       inner join Customer cs
    on cs.ID = c.CustomerID
 where p.RowState in (1, 3)   
GO



