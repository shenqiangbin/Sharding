/*
4100w id范围：41020053-41020062
*/
SELECT * FROM log limit 41000000,10;  /* 22s */ 
SELECT * FROM log WHERE id >= (SELECT id FROM log LIMIT 41000000, 1) LIMIT 10;  /* 14s 快5、6秒的样子 */  
SELECT * FROM log WHERE id BETWEEN 41000000 AND 41000010; /* 直接出来 */  

/* 添加排序 （主键id的排序） */
SELECT * FROM log order by id desc limit 41000000,10;  /* 43s */ 
SELECT * FROM log order by id asc limit 41000000,10;  /* 20s  结论：orderby 的顺序也有关系 */ 

SELECT * FROM log WHERE id >= (SELECT id FROM log LIMIT 41000000, 1) order by id desc LIMIT 10;  /* 14s 快5、6秒的样子 */  
SELECT * FROM log WHERE id >= (SELECT id FROM log LIMIT 41000000, 1) order by id asc LIMIT 10;  /* 14s 快5、6秒的样子 */  

SELECT * FROM log WHERE id BETWEEN 41000000 AND 41000010 order by id desc; /* 直接出来 */  
SELECT * FROM log WHERE id BETWEEN 41000000 AND 41000010 order by id asc; /* 直接出来 */  

/* 添加排序 （日期的排序 日期没有索引） */
SELECT * FROM log order by date desc limit 41000000,10;  /* 43s */ 
SELECT * FROM log order by date asc limit 41000000,10;  /* 20s  结论：orderby 的顺序也有关系 */ 

SELECT * FROM log WHERE id >= (SELECT id FROM log LIMIT 41000000, 1) order by date desc LIMIT 10;  /* 14s 快5、6秒的样子 */  
SELECT * FROM log WHERE id >= (SELECT id FROM log LIMIT 41000000, 1) order by date asc LIMIT 10;  /* 14s 快5、6秒的样子 */  

SELECT * FROM log WHERE id BETWEEN 41000000 AND 41000010 order by date desc; /* 直接出来 */  
SELECT * FROM log WHERE id BETWEEN 41000000 AND 41000010 order by date asc; /* 直接出来 */ 

/*查询速度快是因为命中了索引，但如果id不连续，就成了问题*/

SELECT * FROM log WHERE 1=1 and message like '%909948' order by id asc limit 0,10;  /* 8s */
SELECT * FROM log WHERE 1=1 and message like '%909948' order by id asc limit 0,10;  /* 8s */


select count(0) from log where 1=1 and message like '%909948';


select * from (select *,(@rowNum:=@rowNum+1) as Number from ( SELECT * FROM log where 1 = 1 order by date desc limit 41000000, 10 )t,(Select (@rowNum :=0) ) b) tt;
SELECT * FROM log where 1 = 1 order by date desc limit 41000000, 10; /* 199s */
SELECT * FROM log where 1 = 1 limit 41000000, 10; /* 21s */

/*创建索引之后*/
CREATE INDEX `idx_log_date` ON `log` (date desc) COMMENT '' ALGORITHM DEFAULT LOCK DEFAULT; /* 105s */
SELECT * FROM log where 1 = 1 order by date desc limit 41000000, 10; /* 199s */
SELECT * FROM log WHERE id >= (SELECT id FROM log LIMIT 41000000, 1)   order by date desc LIMIT 10;

