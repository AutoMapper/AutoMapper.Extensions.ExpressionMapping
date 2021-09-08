using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    using AutoMapper.Internal;

    public class XpressionMapperTests
    {
        public XpressionMapperTests()
        {
            SetupAutoMapper();
            SetupQueryableCollection();
        }

        #region Tests
        [Fact]
        public void Map_includes_list()
        {
            //Arrange
            ICollection<Expression<Func<UserModel, object>>> selections = new List<Expression<Func<UserModel, object>>>() { s => s.AccountModel.Bal, s => s.AccountName };

            //Act
            IList<Expression<Func<User, object>>> selectionsMapped = mapper.MapIncludesList<Expression<Func<User, object>>>(selections).ToList();
            List<object> accounts = Users.Select(selectionsMapped[0]).ToList();
            List<object> branches = Users.Select(selectionsMapped[1]).ToList();

            //Assert
            Assert.True(accounts.Count == 2 && branches.Count == 2);
        }

        [Fact]
        public void Map_includes_list_with_select()
        {
            //Arrange
            ICollection<Expression<Func<UserModel, object>>> selections = new List<Expression<Func<UserModel, object>>>() { s => s.AccountModel.Bal, s => s.AccountName, s => s.AccountModel.ThingModels.Select<ThingModel, object>(x => x.Color) };

            //Act
            IList<Expression<Func<User, object>>> selectionsMapped = mapper.MapIncludesList<Expression<Func<User, object>>>(selections).ToList();
            List<object> accounts = Users.Select(selectionsMapped[0]).ToList();
            List<object> branches = Users.Select(selectionsMapped[1]).ToList();
            List<object> cars = Users.Select(selectionsMapped[2]).SelectMany(o => (o as IEnumerable<object>)).ToList();

            //Assert
            Assert.True(cars.Count == 4 && accounts.Count == 2 && branches.Count == 2);
        }

        [Fact]
        public void Map_includes_with_value_types()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountModel.Bal;

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> accounts = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 2);
        }

        [Fact]
        public void Map_includes_with_string()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountName;

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> accounts = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 2);
        }

        [Fact]
        public void Map_collection_includes_with_flattened_string()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<string>>> selection = s => s.AccountModel.ThingModels.Select<ThingModel, string>(x => x.Color);

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> listOfCarLists = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(listOfCarLists.Count == 2);
        }

        [Fact]
        public void Map_collection_includes_with_flattened_collection()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountModel.ThingModels;

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> listOfCarLists = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(listOfCarLists.Count == 2);
        }

        [Fact]
        public void Map_collection_includes_with_flattened_collection_return_type_not_converted()
        {
            //Arrange
            Expression<Func<UserModel, object>> selection = s => s.AccountModel.ThingModels;

            //Act
            Expression<Func<User, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, object>>>(selection);
            List<object> listOfCarLists = Users.Select(selectionMapped).ToList();

            //Assert
            Assert.True(selectionMapped.ToString() == "s => s.Account.Things");
        }

        [Fact]
        public void Map_includes_trim_string_nested_in_select_using_object()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<object>>> selection = s => s.AccountModel.ThingModels.Select<ThingModel, object>(x => x.Color);

            //Act
            Expression<Func<User, IEnumerable<object>>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, IEnumerable<object>>>>(selection);
            List<object> cars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(cars.Count == 4);
        }

        [Fact]
        public void Map_includes_trim_string_nested_in_select_using_explicit_types()
        {//Probebly want to be careful about mapping strings or value types to reference types.  What it there are multiple strings in the expression?
            //Arrange
            Expression<Func<UserModel, IEnumerable<string>>> selection = s => s.AccountModel.ThingModels.Select<ThingModel, string>(x => x.Color);

            //Act
            Expression<Func<User, IEnumerable<Car>>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<User, IEnumerable<Car>>>>(selection);
            List<Car> cars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(cars.Count == 4);
        }

        [Fact]
        public void Map_object_type_change()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.LoggedOn == "Y";

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map_works_with_members_from_interfaces()
        {
            //Arrange
            Expression<Func<IUserModel, bool>> selection = s => s.LoggedOn == "Y";

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map_works_with_members_from_base_interfaces()
        {
            //Arrange
            Expression<Func<IUserModel, bool>> selection = s => s.Id == 14;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map_object_type_change_again()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.IsOverEighty;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 0);
        }

        [Fact]
        public void Uses_the_correct_Add_expression_when_mapping_string_plus_operator()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s.FullName + "FFF" == "";

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.MapExpression<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 0);
        }

        [Fact]
        public void Map__object_including_child_and_grandchild()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.Bal > 555.20;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 2);
        }

        [Fact]
        public void Map__object_including_child_and_grandchild_with_conditional_filter()
        {
            //Arrange
            ParameterExpression userParam = Expression.Parameter(typeof(UserModel), "s");
            MemberExpression accountModelProperty = Expression.MakeMemberAccess(userParam, TypeExtensions.GetFieldOrProperty(typeof(UserModel), "AccountModel"));
            MemberExpression branchModelProperty = Expression.MakeMemberAccess(accountModelProperty, TypeExtensions.GetFieldOrProperty(typeof(AccountModel), "Branch"));
            MemberExpression nameProperty = Expression.MakeMemberAccess(branchModelProperty, TypeExtensions.GetFieldOrProperty(typeof(BranchModel), "Name"));

            //{s => (IIF((IIF((s.AccountModel == null), null, s.AccountModel.Branch) == null), null, s.AccountModel.Branch.Name) == "Leeds")}
            Expression<Func<UserModel, bool>> selection = Expression.Lambda<Func<UserModel, bool>>
            (
                Expression.Equal
                (
                    Expression.Condition
                    (
                        Expression.Equal
                        (
                            Expression.Condition
                            (
                                Expression.Equal
                                (
                                    accountModelProperty,
                                    Expression.Constant(null, typeof(AccountModel))
                                ),
                                Expression.Constant(null, typeof(BranchModel)),
                                branchModelProperty
                            ),
                            Expression.Constant(null, typeof(BranchModel))
                        ),
                        Expression.Constant(null, typeof(string)),
                        nameProperty
                    ),
                    Expression.Constant("Park Row", typeof(string))
                ),
                userParam
            );

            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map_object_when_null_values_are_typed()
        {
            //Arrange
            //Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.Bal > 555.20;
            ParameterExpression userParam = Expression.Parameter(typeof(UserModel), "s");
            MemberExpression accountModelProperty = Expression.MakeMemberAccess(userParam, TypeExtensions.GetFieldOrProperty(typeof(UserModel), "AccountModel"));
            Expression<Func<UserModel, bool>> selection = Expression.Lambda<Func<UserModel, bool>>
                (
                    Expression.AndAlso
                        (
                            Expression.AndAlso
                                (
                                    Expression.NotEqual(userParam, Expression.Constant(null, typeof(UserModel))),
                                    Expression.NotEqual(accountModelProperty, Expression.Constant(null, typeof(AccountModel)))
                                ),
                            Expression.GreaterThan
                                (
                                    Expression.MakeMemberAccess(accountModelProperty, TypeExtensions.GetFieldOrProperty(typeof(AccountModel), "Bal")),
                                    Expression.Constant(555.20, typeof(double))
                                )
                        ),
                    userParam
                );

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.MapExpression<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 2);
        }

        [Fact]
        public void Map_expression_should_throws_exception_when_mapping_is_missed()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Car, CarModel>();
            });

            var customMapper = config.CreateMapper();

            Expression<Func<Car, bool>> expression = car => car.Year == 2017;

            // Act + Assert
            new Action(() => customMapper.Map<Expression<Func<CarModel, bool>>>(expression))
                .ShouldThrowException<AutoMapperMappingException>(
                    exception =>
                    {
                        exception.InnerException.ShouldNotBeNull();
                        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
                        exception.InnerException.Message.ShouldContain("CreateMap<CarModel, Car>", Case.Insensitive);
                    });
        }

        [Fact]
        public void Map_expression_should_throws_exception_when_mapping_for_property_is_missed()
        {
            // Arrange    
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<ThingModel, Thing>();
            });

            var customMapper = config.CreateMapper();

            Expression<Func<Thing, object>> expression = thing => thing.Foo;

            // Act + Assert
            new Action(() => customMapper.Map<Expression<Func<ThingModel, object>>>(expression))
                .ShouldThrowException<AutoMapperMappingException>(
                    exception =>
                    {
                        exception.InnerException.ShouldNotBeNull();
                        exception.InnerException.ShouldBeOfType<AutoMapperConfigurationException>();

                        var inner = exception.InnerException as AutoMapperConfigurationException;
                        inner.ShouldNotBeNull();

                        var error = inner.Errors.ShouldHaveSingleItem();
                        error.ShouldNotBeNull();

                        error.TypeMap.ShouldNotBeNull();
                        error.TypeMap.DestinationType.ShouldBe(typeof(Thing));
                        error.TypeMap.SourceType.ShouldBe(typeof(ThingModel));

                        var missedProperty = error.UnmappedPropertyNames.ShouldHaveSingleItem();
                        missedProperty.ShouldBe("Foo");
                    });
        }

        [Fact]
        public void Map_project_truncated_time()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel != null && s.AccountModel.DateCreated == DateTime.Now;

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 0);
        }

        [Fact]
        public void Map_projection()
        {
            //Arrange
            Expression<Func<UserModel, bool>> selection = s => s != null && s.AccountModel.Description.StartsWith("B");

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map__flattened_property()
        {
            //Arrange
            int age = 25;
            Expression<Func<UserModel, bool>> selection = s => ((s != null ? s.AccountName : null) ?? "").ToLower().StartsWith("P".ToLower()) && ((s.AgeInYears == age) && s.IsActive);

            //Act
            Expression<Func<User, bool>> selectionMapped = mapper.Map<Expression<Func<User, bool>>>(selection);
            List<User> users = Users.Where(selectionMapped).ToList();

            //Assert
            Assert.True(users.Count == 1);
        }

        [Fact]
        public void Map__select_method()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<string>>> selection = s => s.AccountModel.ThingModels.Select(x => x.BarModel).Where(b => b.EndsWith("3"));

            //Act
            Expression<Func<User, IEnumerable<string>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<string>>>>(selection);
            List<string> bars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(bars.Count == 1);
        }

        [Fact]
        public void Map__select_method_projecting_to_anonymous_type()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<object>>> selection = s => s.AccountModel.ThingModels.Select(x => new { MM = x.BarModel }).Where(b => b.MM.EndsWith("3"));

            //Act
            Expression<Func<User, IEnumerable<object>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<object>>>>(selection);
            List<object> bars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(bars.Count == 1);
        }

        [Fact]
        public void Map__select_method_projecting_to_model_type()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<ThingModel>>> selection = s => s.AccountModel.ThingModels.Select(x => new ThingModel { Car = x.Car }).Where(b => b.Color.EndsWith("e"));

            //Act
            Expression<Func<User, IEnumerable<Thing>>> selectionMapped = mapper.MapExpression<Expression<Func<User, IEnumerable<Thing>>>>(selection);
            List<Thing> things = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(things.Count == 2);
        }

        [Fact]
        public void Map_member_init()
        {
            //Arrange
            Expression<Func<ThingModel, ThingModel>> selection = s => new ThingModel { Car = s.Car };

            //Act
            Expression<Func<Thing, Thing>> selectionMapped = mapper.MapExpression<Expression<Func<Thing, Thing>>>(selection);

            //Assert
            Assert.NotNull(selectionMapped);
        }

        [Fact]
        public void Map_member_init_when_source_member_is_enum_and_destination_member_is_int()
        {
            //Arrange
            Expression<Func<ModelWithEnumMembers, ModelWithEnumMembers>> selection = m => new ModelWithEnumMembers { EnumValue = m.EnumValue };

            //Act
            var selectionMapped = mapper.MapExpression<Expression<Func<EntityWithIntMembers, EntityWithIntMembers>>>(selection);

            //Assert
            Assert.NotNull(selectionMapped);
        }

        [Fact]
        public void Map_member_init_when_source_member_is_nullable_enum_and_destination_member_is_int()
        {
            //Arrange
            Expression<Func<ModelWithEnumMembers, ModelWithEnumMembers>> selection = m => new ModelWithEnumMembers { NullableEnumValue = m.NullableEnumValue };

            //Act
            var selectionMapped = mapper.MapExpression<Expression<Func<EntityWithIntMembers, EntityWithIntMembers>>>(selection);

            //Assert
            Assert.NotNull(selectionMapped);
        }

        [Fact]
        public void Map_Constructor_NoParams()
        {
            //Arrange
            Expression<Func<ThingModel, ThingModel>> selection = s => new ThingModel();

            //Act
            Expression<Func<Thing, Thing>> selectionMapped = mapper.MapExpression<Expression<Func<Thing, Thing>>>(selection);

            //Assert
            Assert.NotNull(selectionMapped);
        }

        [Fact]
        public void Map__select_method_where_parent_type_is_grandchild_type()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<int>>> selection = s => s.AccountModel.UserModels.Select(x => x.AgeInYears);

            //Act
            Expression<Func<User, IEnumerable<int>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<int>>>>(selection);
            List<int> bars = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(bars.Count == 2);
        }

        [Fact]
        public void Map_where_method()
        {
            //Arrange
            Expression<Func<UserModel, IEnumerable<ThingModel>>> selection = s => s.AccountModel.ThingModels.Where(x => x.BarModel == s.AccountName);

            //Act
            Expression<Func<User, IEnumerable<Thing>>> selectionMapped = mapper.Map<Expression<Func<User, IEnumerable<Thing>>>>(selection);
            List<Thing> things = Users.SelectMany(selectionMapped).ToList();

            //Assert
            Assert.True(things.Count == 0);
        }

        [Fact]
        public void Map_where_multiple_arguments()
        {
            //Arrange
            Expression<Func<UserModel, AccountModel, object>> selection = (u, s) => u.FullName == s.Bal.ToString();

            //Act
            Expression<Func<User, Account, object>> selectionMapped = mapper.Map<Expression<Func<User, Account, object>>>(selection);
            object val = selectionMapped.Compile().Invoke(Users.ToList()[0], Users.ToList()[0].Account);

            //Assert
            Assert.False((bool)val);
        }

        [Fact]
        public void Map_orderBy_thenBy_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<UserModel>>> exp = q => q.OrderByDescending(s => s.Id).ThenBy(s => s.FullName);

            //Act
            Expression<Func<IQueryable<User>, IQueryable<User>>> expMapped = mapper.Map<Expression<Func<IQueryable<User>, IQueryable<User>>>>(exp);
            List<User> users = expMapped.Compile().Invoke(Users).ToList();

            //Assert
            Assert.True(users[0].UserId == 14);
        }

        [Fact]
        public void Map_orderBy_thenBy_GroupBy_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<IGrouping<int, UserModel>>>> grouped = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName).GroupBy(s => s.AgeInYears);

            //Act
            Expression<Func<IQueryable<User>, IQueryable<IGrouping<int, User>>>> expMapped = mapper.Map<Expression<Func<IQueryable<User>, IQueryable<IGrouping<int, User>>>>>(grouped);
            List<IGrouping<int, User>> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].Count() == 2);
        }

        [Fact]
        public void Map_orderBy_thenBy_GroupBy_Select_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<object>>> grouped = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName).GroupBy(s => s.AgeInYears).Select(grp => new { Id = grp.Key, Count = grp.Count() });

            //Act
            Expression<Func<IQueryable<User>, IQueryable<object>>> expMapped = mapper.MapExpression<Expression<Func<IQueryable<User>, IQueryable<object>>>>(grouped);
            List<dynamic> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].Count == 2);
        }

        [Fact]
        public void Map_orderBy_thenBy_GroupBy_SelectMany_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IQueryable<UserModel>>> grouped = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName).GroupBy(s => s.AgeInYears).SelectMany(grp => grp);

            //Act
            Expression<Func<IQueryable<User>, IQueryable<User>>> expMapped = mapper.MapExpression<Expression<Func<IQueryable<User>, IQueryable<User>>>>(grouped);
            List<User> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users.Count == 2);
        }

        [Fact]
        public void Map_orderBy_thenBy_To_Dictionary_Select_expression()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IEnumerable<object>>> grouped = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName).ToDictionary(kvp => kvp.Id).Select(grp => new { Id = grp.Key, Name = grp.Value.FullName });

            //Act
            Expression<Func<IQueryable<User>, IEnumerable<object>>> expMapped = mapper.MapExpression<Expression<Func<IQueryable<User>, IEnumerable<object>>>>(grouped);
            List<dynamic> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].Id == 11);
        }

        [Fact]
        public void Map_orderBy_thenBy_To_Dictionary_Select_expression_without_generic_types()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IEnumerable<object>>> grouped = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName).ToDictionary(kvp => kvp.Id).Select(grp => new { Id = grp.Key, Name = grp.Value.FullName });

            //Act
            Expression<Func<IQueryable<User>, IEnumerable<object>>> expMapped = (Expression<Func<IQueryable<User>, IEnumerable<object>>>)mapper.MapExpression
            (
                grouped, 
                typeof(Expression<Func<IQueryable<UserModel>, IEnumerable<object>>>), 
                typeof(Expression<Func<IQueryable<User>, IEnumerable<object>>>)
            );

            List<dynamic> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].Id == 11);
        }

        [Fact]
        public void Map_to_anonymous_type_when_init_member_is_not_a_literal()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IEnumerable<object>>> expression = q => q.OrderBy(s => s.Id).Select(u => new { UserId = u.Id, Account = u.AccountModel });

            //Act
            Expression<Func<IQueryable<User>, IEnumerable<object>>> expMapped = (Expression<Func<IQueryable<User>, IEnumerable<object>>>)mapper.MapExpression
            (
                expression,
                typeof(Expression<Func<IQueryable<UserModel>, IEnumerable<object>>>),
                typeof(Expression<Func<IQueryable<User>, IEnumerable<object>>>)
            );

            List<dynamic> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].UserId == 11);
            Assert.True(users[0].Account.Balance == 150000);
        }

        [Fact]
        public void Map_to_anonymous_type_when_init_member_is_not_a_literal_and_parameter_is_anonymous_type()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IEnumerable<object>>> expression = q => q.OrderBy(s => s.Id).Select(u => new { UserId = u.Id, Account = u.AccountModel }).Where(a => a.Account.Bal != -1);

            //Act
            Expression<Func<IQueryable<User>, IEnumerable<object>>> expMapped = (Expression<Func<IQueryable<User>, IEnumerable<object>>>)mapper.MapExpression
            (
                expression,
                typeof(Expression<Func<IQueryable<UserModel>, IEnumerable<object>>>),
                typeof(Expression<Func<IQueryable<User>, IEnumerable<object>>>)
            );

            List<dynamic> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].UserId == 11);
            Assert.True(users[0].Account.Balance == 150000);
        }

        [Fact]
        public void Map_to_anonymous_type_when_init_member_is_not_a_literal_with_navigation_property()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, IEnumerable<object>>> expression = q => q.OrderBy(s => s.Id).Select(u => new { UserId = u.Id, Branch = u.AccountModel.Branch });

            //Act
            Expression<Func<IQueryable<User>, IEnumerable<object>>> expMapped = (Expression<Func<IQueryable<User>, IEnumerable<object>>>)mapper.MapExpression
            (
                expression,
                typeof(Expression<Func<IQueryable<UserModel>, IEnumerable<object>>>),
                typeof(Expression<Func<IQueryable<User>, IEnumerable<object>>>)
            );

            List<dynamic> users = expMapped.Compile().Invoke(Users).ToList();

            Assert.True(users[0].UserId == 11);
            Assert.True(users[0].Branch.Name == "Head Row");
        }

        [Fact]
        public void Map_dynamic_return_type()
        {
            //Arrange
            Expression<Func<IQueryable<UserModel>, dynamic>> exp = q => q.OrderBy(s => s.Id).ThenBy(s => s.FullName);

            //Act
            Expression<Func<IQueryable<User>, dynamic>> expMapped = mapper.Map<Expression<Func<IQueryable<User>, dynamic>>>(exp);
            List<User> users = Enumerable.ToList(expMapped.Compile().Invoke(Users));

            //Assert
            Assert.True(users[0].UserId == 11);
        }

        [Fact]
        public void Map_1821_when_mapping_expression_with_a_property_that_maps_to_a_string_type()
        {
            //Arrange
            Expression<Func<Item, object>> exp = x => x.Name;

            //Act
            Expression<Func<ItemDto, object>> expMapped = mapper.Map<Expression<Func<ItemDto, object>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_1893_null_exception_with_deflattening()
        {
            //Arrange
            Expression<Func<OrderLineDTO, bool>> dtoExpression = dto => dto.Item.Name == "Item #1";

            //Act
            Expression<Func<OrderLine, bool>> expression = mapper.Map<Expression<Func<OrderLine, bool>>>(dtoExpression);

            //Assert
            Assert.NotNull(expression);
        }

        [Fact]
        public void Map_parentDto_to_parent()
        {
            //Arrange
            Expression<Func<ParentDTO, bool>> exp = p => (p.DateTime.Year.ToString() != String.Empty);
            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Can_map_expression_where_parent_of_member_expression_is_typeAsExpression()
        {
            //Arrange
            Parent[] parents = new Parent[]
            {
                new Parent {  DateTimeObject = new DateTime?(new DateTime(2019, 9, 7))}
            };
            Expression<Func<ParentDTO, bool>> exp = p => (p.DateTimeObject as DateTime?).Value.Year.ToString() == "2019";

            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.Equal(1, parents.AsQueryable<Parent>().Count(expMapped));
        }

        [Fact]
        public void Map_parentDto_to_parent_with_index_argument()
        {
            //Arrange
            var ids = new[] { 4, 5 };
            Expression<Func<ParentDTO, bool>> exp = p => p.Children.Where((c, i) => c.ID_ > 4).Any(c => ids.Contains(c.ID_));
            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_accountModel_to_account()
        {
            //Arrange
            Expression<Func<AccountModel, bool>> exp = p => (p.DateCreated.Year.ToString() != String.Empty);

            //Act
            Expression<Func<Account, bool>> expMapped = mapper.Map<Expression<Func<Account, bool>>>(exp);
            List<Account> accounts = Users.Select(u => u.Account).Where(expMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 2);
        }

        [Fact]
        public void Map_accountModel_to_account_with_local_nullables()
        {
            DateTime? firstReleaseDate = null;
            DateTime? lastReleaseDate = null;

            Expression<Func<AccountModel, bool>> exp = x => (firstReleaseDate == null || x.DateCreated >= firstReleaseDate) &&
                                      (lastReleaseDate == null || x.DateCreated <= lastReleaseDate);

            //Act
            Expression<Func<Account, bool>> expMapped = mapper.MapExpression<Expression<Func<Account, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_ItemDto_to_ItemDto_with_local_nullables()
        {
            DateTime? firstReleaseDate = new DateTime();
            DateTime? lastReleaseDate = new DateTime();

            Expression<Func<ItemDto, bool>> exp = x => (firstReleaseDate == null || x.CreateDate >= firstReleaseDate) &&
                                      (lastReleaseDate == null || x.CreateDate <= lastReleaseDate);

            //Act
            Expression<Func<Item, bool>> expMapped = mapper.MapExpression<Expression<Func<Item, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_ItemDto_to_ItemDto_with_local_literal_types()
        {
            DateTime firstReleaseDate = new DateTime();
            DateTime lastReleaseDate = new DateTime();

            Expression<Func<ItemDto, bool>> exp = x => (firstReleaseDate == null || x.CreateDate >= firstReleaseDate) &&
                                      (lastReleaseDate == null || x.CreateDate <= lastReleaseDate);

            //Act
            Expression<Func<Item, bool>> expMapped = mapper.MapExpression<Expression<Func<Item, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Map_accountModel_to_account_with_null_checks_against_value_types()
        {
            //Arrange
            Expression<Func<AccountModel, bool>> exp = f => (f != null ? f.Id : 0) > 10
             && (f != null && f.DateCreated != null ? f.DateCreated : default(DateTime)) > new DateTime(2007, 02, 17);


            //Act
            Expression<Func<Account, bool>> expMapped = mapper.MapExpression<Expression<Func<Account, bool>>>(exp);
            List<Account> accounts = Users.Select(u => u.Account).Where(expMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 1);
        }

        [Fact]
        public void Map_accountModel_to_account_with_null_checks_against_string_type()
        {
            //Arrange
            Expression<Func<AccountModel, bool>> exp = f => f.Description == null;


            //Act
            Expression<Func<Account, bool>> expMapped = mapper.MapExpression<Expression<Func<Account, bool>>>(exp);
            List<Account> accounts = Users.Select(u => u.Account).Where(expMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 0);
        }

        [Fact]
        public void Map_accountModel_to_account_with_left_null_checks_against_string_type()
        {
            //Arrange
            Expression<Func<AccountModel, bool>> exp = f => null == f.Description;


            //Act
            Expression<Func<Account, bool>> expMapped = mapper.MapExpression<Expression<Func<Account, bool>>>(exp);
            List<Account> accounts = Users.Select(u => u.Account).Where(expMapped).ToList();

            //Assert
            Assert.True(accounts.Count == 0);
        }


        [Fact]
        public void When_use_lambda_statement_with_typemapped_property_being_other_than_first()
        {
            //Arrange
            Expression<Func<ParentDTO, bool>> exp = p => p.Children.AnyParamReverse((c, c2) => c2.ID_ > 4);
            //Act
            Expression<Func<Parent, bool>> expMapped = mapper.Map<Expression<Func<Parent, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Test_Error_when_mapping_nested_expressions_when_enumerable_is_array_2201()
        {
            //Arrange
            Expression<Func<DataClass, bool>> expr = d => d.Children.All(c => c.Data == "abc");

            //Act
            Expression<Func<ModelClass, bool>> expMapped = mapper.Map<Expression<Func<ModelClass, bool>>>(expr);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Can_correctly_type_maps_for_child_properties()
        {
            //Arrange
            Expression<Func<OrderModel, bool>> src = x => x.Items.Any(i => i.Name == "Test");

            //Act
            Expression<Func<OrderEntity, bool>> dest = mapper.Map<Expression<Func<OrderEntity, bool>>>(src);

            //Assert
            Assert.NotNull(dest);
        }

        [Fact]
        public void Can_map_expressions_with_no_parameters()
        {
            //Arrange
            Expression<Func<OptionS>> exp = () => Activator.CreateInstance<OptionS>();

            //Act
            Expression<Func<OptionT>> expmapped = mapper.Map<Expression<Func<OptionS>>, Expression<Func<OptionT>>>(exp);

            //Assert
            Assert.True(expmapped.Type == typeof(Func<OptionT>));
        }


        [Fact]
        public void Can_map_expressions_with_action_independent_of_expression_param()
        {
            //Arrange
            Expression<Action<OptionS>> exp = (s) => CallSomeAction<OptionS>(Activator.CreateInstance<OptionS>());

            //Act
            Expression<Action<OptionT>> expmapped = mapper.MapExpression<Expression<Action<OptionS>>, Expression<Action<OptionT>>>(exp);

            //Assert
            expmapped.Compile()(Activator.CreateInstance<OptionT>());
            Assert.True(this.val.GetType() == typeof(OptionT));
        }

        object val;
        void CallSomeAction<T>(T val)
        {
            this.val = val;
        }

        [Fact]
        public void Can_map_expression_when_mapped_properties_have_a_different_generic_argument_counts()
        {
            //Arrange
            Expression<Func<ListParent, bool>> src = x => x.List.Count == 0;

            //Act
            Expression<Func<ListParentExtension, bool>> dest = mapper.Map<Expression<Func<ListParentExtension, bool>>>(src);

            //Assert
            Assert.NotNull(dest);
        }

        [Fact]
        public void Can_map_expression_when_mapped_when_members_parent_is_a_method()
        {
            //Arrange
            List<EmployeeEntity> empEntity = new List<EmployeeEntity>
            {
                new EmployeeEntity { Id = 1, Name = "Jean-Louis", Age = 39, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-1) } }.ToList() },
                new EmployeeEntity { Id = 2, Name = "Jean-Paul", Age = 32, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-2) } }.ToList() },
                new EmployeeEntity { Id = 3, Name = "Jean-Christophe", Age = 19, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-1) } }.ToList() },
                new EmployeeEntity { Id = 4, Name = "Jean-Marie", Age = 27, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-3) } }.ToList() },
                new EmployeeEntity { Id = 5, Name = "Jean-Marc", Age = 22, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-5) } }.ToList() },
                new EmployeeEntity { Id = 5, Name = "Jean-Pierre", Age = 22, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-5) } }.ToList() },
                new EmployeeEntity { Id = 6, Name = "Christophe", Age = 55, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-1) } }.ToList() },
                new EmployeeEntity { Id = 7, Name = "Marc", Age = 23, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-2) } }.ToList() },
                new EmployeeEntity { Id = 8, Name = "Paul", Age = 38, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-10) }, new EventEntity { EventType = "Stop", EventDate = DateTime.Today.AddYears(-1) } }.ToList() },
                new EmployeeEntity { Id = 9, Name = "Jean", Age = 32, Events = new EventEntity[]{ new EventEntity { EventType = "Start", EventDate = DateTime.Today.AddYears(-10) }, new EventEntity { EventType = "Stop", EventDate = DateTime.Today.AddYears(-2) } }.ToList() },
            };
            Expression<Func<EmployeeModel, bool>> filter = emp =>
                emp.Events.Any(e => e.EventType.Equals("Stop")) &&
                emp.Events.First(e => e.EventType.Equals("Stop")).EventDate < DateTime.Today.AddYears(-1);

            //Act
            Expression<Func<EmployeeEntity, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EmployeeEntity, bool>>>(filter);
            List<EmployeeEntity> res = empEntity.AsQueryable().Where(mappedFilter).ToList();

            //Assert
            Assert.True(res.Count == 1);
        }

        [Fact]
        public void Can_map_expression_with_condittional_logic_while_deflattening()
        {
            Expression<Func<TestDTO, bool>> expr = x => x.NestedClass.NestedField == 1;

            var mappedExpression = mapper.MapExpression<Expression<Func<TestEntity, bool>>>(expr);

            Assert.NotNull(mappedExpression);
        }

        [Fact]
        public void Can_map_expression_with_multiple_destination_parameters_of_the_same_type()
        {
            Expression<Func<CheckDTO, bool>> dtoExpression = c => c.IsLatest;

            Expression<Func<Check, bool>> mappedExpression = mapper.MapExpression<Expression<Func<Check, bool>>>(dtoExpression);

            Assert.Equal
            (//parameters ch and c are of the same type
                "c => ((c.Finished == null) AndAlso Not(c.Part.History.Any(ch => (ch.Started > c.Started))))",
                mappedExpression.ToString()
            );
        }
        #endregion Tests

        private static void SetupAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddMaps(typeof(OrganizationProfile));
            });

            mapper = config.CreateMapper();
        }

        static IMapper mapper;

        private static void SetupQueryableCollection()
        {
            Users = new User[]
            {
                new User
                {
                    Account = new Account
                    {
                        Balance = 150000,
                        Branch = new Branch { Id = 1, Name = "Head Row" },
                        CreateDate = new DateTime(2011, 2, 2),
                        Id = 12,
                        Location = new Location { City = "Leeds", Latitude = 53.8008, Longitude = -1.5491 },
                        Number = "232232232",
                        Things = new Thing[]
                        {
                            new Thing { Bar = "Bar", Car = new Car { Color = "Black", Year = 2014 } },
                            new Thing { Bar = "Bar2", Car = new Car { Color = "White", Year = 2015 } }
                        },
                        Type = "Personal",
                        Users = new User[]
                        {
                            new User
                            {
                                Active = true,
                                Age = 25,
                                FirstName = "John",
                                LastName = "Smith",
                                IsLoggedOn = true,
                                UserId = 11
                            }
                        }
                    },
                    Active = true,
                    Age = 25,
                    FirstName = "John",
                    LastName = "Smith",
                    IsLoggedOn = true,
                    UserId = 11
                },
                new User
                {
                    Account = new Account
                    {
                        Balance = 200000,
                        Branch = new Branch { Id = 1, Name = "Park Row" },
                        CreateDate = new DateTime(2012, 3, 3),
                        Id = 7,
                        Location = new Location { City = "Leeds", Latitude = 53.8008, Longitude = -1.5491 },
                        Number = "444555444",
                        Things = new Thing[]
                        {
                            new Thing { Bar = "Bar3", Car = new Car { Color = "Black", Year = 2014 } },
                            new Thing { Bar = "Bar4", Car = new Car { Color = "White", Year = 2015 } }
                        },
                        Type = "Business",
                        Users = new User[] 
                        {
                            new User
                            {
                                Active = true,
                                Age = 25,
                                FirstName = "Jack",
                                LastName = "Spratt",
                                IsLoggedOn = false,
                                UserId = 14
                            }
                        }
                    },
                    Active = true,
                    Age = 25,
                    FirstName = "Jack",
                    LastName = "Spratt",
                    IsLoggedOn = false,
                    UserId = 14
                }
            }.AsQueryable<User>();
        }

        private static IQueryable<User> Users { get; set; }
    }

    public class OptionS
    {
        public static OptionS GetNew() => new OptionS();
    }

    public class OptionT
    {
        public static OptionT GetNew() => new OptionT();
    }

    public class Account
    {
        public Account()
        {
            Things = new List<Thing>();
        }
        public int Id { get; set; }
        public double Balance { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public DateTime CreateDate { get; set; }
        public Location Location { get; set; }
        public Branch Branch { get; set; }
        public ICollection<Thing> Things { get; set; }
        public ICollection<User> Users { get; set; }
    }

    public class AccountModel
    {
        public AccountModel()
        {
            ThingModels = new List<ThingModel>();
        }
        public int Id { get; set; }
        public double Bal { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public BranchModel Branch { get; set; }
        public ICollection<ThingModel> ThingModels { get; set; }
        public ICollection<UserModel> UserModels { get; set; }
    }

    public class Thing
    {
        public int Foo { get; set; }
        public string Bar { get; set; }
        public Car Car { get; set; }
    }

    public class ThingModel
    {
        public int FooModel { get; set; }
        public string BarModel { get; set; }
        public string Color { get; set; }
        public CarModel Car { get; set; }
    }

    public class Car
    {
        public string Color { get; set; }
        public int Year { get; set; }
    }

    public class CarModel
    {
        public string Color { get; set; }
        public int Year { get; set; }
    }

    public class Location
    {
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BranchModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UserVM
    {
        public string Name { get; set; }
        public bool IsLoggedOn { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public Account Account { get; set; }
    }
    public class UserM
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsLoggedOn { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public Account Account { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsLoggedOn { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public Account Account { get; set; }
    }

    public class UserModel : IUserModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string AccountName { get; set; }
        public bool IsOverEighty { get; set; }
        public string LoggedOn { get; set; }
        public int AgeInYears { get; set; }
        public bool IsActive { get; set; }
        public AccountModel AccountModel { get; set; }
    }

    public interface IIdentifiable
    {
        int Id { get; set; }
    }

    public interface IUserModel : IIdentifiable
    {
        string FullName { get; set; }
        string AccountName { get; set; }
        bool IsOverEighty { get; set; }
        string LoggedOn { get; set; }
        int AgeInYears { get; set; }
        bool IsActive { get; set; }
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderLineDTO
    {
        public int Id { get; set; }
        public ItemDTO Item { get; set; }
        public int Quantity { get; set; }
    }

    public class ItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
    }


    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }
    }

    public class GrandParentDTO
    {
        public ParentDTO Parent { get; set; }
    }
    public class ParentDTO
    {
        public ICollection<ChildDTO> Children { get; set; }
        public ChildDTO Child { get; set; }
        public DateTime DateTime { get; set; }
        public object DateTimeObject { get; set; }
    }

    public class ChildDTO
    {
        public ParentDTO Parent { get; set; }
        public ChildDTO GrandChild { get; set; }
        public int ID_ { get; set; }
        public int? IDs { get; set; }
        public int? ID2 { get; set; }
    }

    public class GrandParent
    {
        public Parent Parent { get; set; }
    }

    public class Parent
    {
        public ICollection<Child> Children { get; set; }

        private Child _child;
        public Child Child
        {
            get { return _child; }
            set
            {
                _child = value;
                _child.Parent = this;
            }
        }
        public DateTime DateTime { get; set; }
        public object DateTimeObject { get; set; }
    }

    public class Child
    {
        private Parent _parent;
        public Parent Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                if (GrandChild != null)
                    GrandChild.Parent = _parent;
            }
        }

        public int ID { get; set; }
        public Child GrandChild { get; set; }
        public int IDs { get; set; }
        public int? ID2 { get; set; }
    }

    public class ModelClass
    {
        public int ID { get; set; }
        public ChildModelClass[] Children { get; set; }
    }

    public class ChildModelClass
    {
        public string Data { get; set; }
    }

    public class DataClass
    {
        public int ID { get; set; }
        public ChildDataClass[] Children { get; set; }
    }

    public class ChildDataClass
    {
        public string Data { get; set; }
    }

    class OrderModel
    {
        public IEnumerable<ItemModel> Items { get; set; }
    }

    class ItemModel
    {
        public string Name { get; set; }
    }

    class OrderEntity
    {
        public OrderData OrderData { get; set; }
    }

    class OrderData
    {
        public IEnumerable<ItemEntity> Items { get; set; }
    }

    class ItemEntity
    {
        public string Name { get; set; }
    }

    class ListParentExtension 
    {
        public ListExtension List { get; set; }
    }

    class ListParent : List<string>
    {
        public List<string> List { get; set; }
    }

    class ListExtension : List<string>
    {
    }

    internal class EmployeeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<EventEntity> Events { get; set; }
    }

    internal class EventEntity
    {
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
    }


    internal class EmployeeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<EventModel> Events { get; set; }
    }

    internal class EventModel
    {
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
    }

    public class TestEntity
    {
        public Guid Id { get; set; }
        public int ConditionField { get; set; }
        public int ToBeNestedField { get; set; }
    }

    public class TestDTO
    {
        public Guid Id { get; set; }
        public TestDTONestedClass NestedClass { get; set; }
    }
    public class TestDTONestedClass
    {
        public int? NestedField { get; set; }
    }

    public enum TestEnum
    {
        Prop0 = 0,
        Prop1 = 1
    }

    public class EntityWithIntMembers
    {
        public int IntValue { get; set; }
        public int OtherIntValue { get; set; }
    }

    public class ModelWithEnumMembers
    {
        public TestEnum EnumValue { get; set; }
        public TestEnum? NullableEnumValue { get; set; }
    }

    public class Check
    {
        public DateTime? Finished { get; set; }
        public Part Part { get; set; }
        public DateTime Started { get; set; }
    }

    public class CheckDTO
    {
        public bool IsLatest { get; set; }
        public Guid Part { get; set; }
    }

    public class Part
    {
        public List<Check> History { get; } = new List<Check>();
        public Guid ID { get; } = Guid.NewGuid();
    }

    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Check, CheckDTO>()
                .ForMember(dest => dest.Part, c => c.MapFrom(src => src.Part.ID))
                .ForMember(dest => dest.IsLatest, c => c.MapFrom(src => src.Finished == null && !src.Part.History.Any(ch => ch.Started > src.Started)));

            CreateMap<ChildModelClass, ChildDataClass>().ReverseMap();
            CreateMap<ModelClass, DataClass>().ReverseMap();

            CreateMap<User, UserModel>()
                    .ForMember(d => d.Id, opt => opt.MapFrom(s => s.UserId))
                    .ForMember(d => d.FullName, opt => opt.MapFrom(s => string.Concat(s.FirstName, " ", s.LastName)))
                    .ForMember(d => d.LoggedOn, opt => opt.MapFrom(s => s.IsLoggedOn ? "Y" : "N"))
                    .ForMember(d => d.IsOverEighty, opt => opt.MapFrom(s => s.Age > 80))
                    .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account == null ? string.Empty : string.Concat(s.Account.Branch.Name, " ", s.Account.Branch.Id.ToString())))
                    .ForMember(d => d.AgeInYears, opt => opt.MapFrom(s => s.Age))
                    .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Active))
                    .ForMember(d => d.AccountModel, opt => opt.MapFrom(s => s.Account));

            CreateMap<UserModel, User>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.IsLoggedOn, opt => opt.MapFrom(s => s.LoggedOn.ToUpper() == "Y"))
                .ForMember(d => d.Age, opt => opt.MapFrom(s => s.AgeInYears))
                .ForMember(d => d.Active, opt => opt.MapFrom(s => s.IsActive))
                .ForMember(d => d.Account, opt => opt.MapFrom(s => s.AccountModel));


            CreateMap<User, IUserModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => string.Concat(s.FirstName, " ", s.LastName)))
                .ForMember(d => d.LoggedOn, opt => opt.MapFrom(s => s.IsLoggedOn ? "Y" : "N"))
                .ForMember(d => d.IsOverEighty, opt => opt.MapFrom(s => s.Age > 80))
                .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account == null ? string.Empty : string.Concat(s.Account.Branch.Name, " ", s.Account.Branch.Id.ToString())))
                .ForMember(d => d.AgeInYears, opt => opt.MapFrom(s => s.Age))
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Active));

            CreateMap<IUserModel, User>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.IsLoggedOn, opt => opt.MapFrom(s => s.LoggedOn.ToUpper() == "Y"))
                .ForMember(d => d.Age, opt => opt.MapFrom(s => s.AgeInYears))
                .ForMember(d => d.Active, opt => opt.MapFrom(s => s.IsActive));


            CreateMap<Account, AccountModel>()
                .ForMember(d => d.Bal, opt => opt.MapFrom(s => s.Balance))
                .ForMember(d => d.DateCreated, opt => opt.MapFrom(s => Helpers.TruncateTime(s.CreateDate).Value))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => string.Concat(s.Type, " ", s.Number)))
                .ForMember(d => d.ThingModels, opt => opt.MapFrom(s => s.Things))
                .ForMember(d => d.UserModels, opt => opt.MapFrom(s => s.Users));

            CreateMap<AccountModel, Account>()
                .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Bal))
                .ForMember(d => d.Things, opt => opt.MapFrom(s => s.ThingModels))
                .ForMember(d => d.Users, opt => opt.MapFrom(s => s.UserModels));

            CreateMap<BranchModel, Branch>();

            CreateMap<Thing, ThingModel>()
                .ForMember(d => d.FooModel, opt => opt.MapFrom(s => s.Foo))
                .ForMember(d => d.BarModel, opt => opt.MapFrom(s => s.Bar))
                .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Car.Color));

            CreateMap<ThingModel, Thing>()
                .ForMember(d => d.Foo, opt => opt.MapFrom(s => s.FooModel))
                .ForMember(d => d.Bar, opt => opt.MapFrom(s => s.BarModel));

            CreateMap<ItemDto, Item>()
                    .ForMember(dest => dest.Date, opts => opts.MapFrom(x => x.CreateDate));

            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.CreateDate, opts => opts.MapFrom(x => x.Date));

            CreateMap<OrderLine, ItemDTO>()
                .ForMember(dto => dto.Name, conf => conf.MapFrom(src => src.ItemName));
            CreateMap<OrderLine, OrderLineDTO>()
                .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol));
            CreateMap<OrderLineDTO, OrderLine>()
                .ForMember(ol => ol.ItemName, conf => conf.MapFrom(dto => dto.Item.Name));

            CreateMap<GrandParent, GrandParentDTO>().ReverseMap();
            CreateMap<Parent, ParentDTO>()
                .ForMember(dest => dest.DateTime, opts => opts.MapFrom(x => x.DateTime))
                .ReverseMap()
                .ForMember(dest => dest.DateTime, opts => opts.MapFrom(x => x.DateTime));
            CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID_, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID_));

            CreateMap<OrderEntity, OrderModel>()
                    .ForMember(x => x.Items, opt => opt.MapFrom(x => x.OrderData.Items));
            CreateMap<ItemEntity, ItemModel>();

            CreateMap<OptionT, OptionS>();

            CreateMap<ListParentExtension, ListParent>()
                .ReverseMap();
            CreateMap<ListExtension, List<string>>()
                .ForMember(d => d.Count, opt => opt.MapFrom(s => s.Count))
                .ReverseMap()
                .ForMember(d => d.Count, opt => opt.MapFrom(s => s.Count));

            CreateMap<Branch, BranchModel>();

            CreateMap<EmployeeModel, EmployeeEntity>().ReverseMap();
            CreateMap<EventModel, EventEntity>().ReverseMap();

            CreateMap<TestEntity, TestDTO>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.NestedClass, opt => opt.MapFrom(src => src.ConditionField == 1 ? src : default));

            CreateMap<TestEntity, TestDTONestedClass>()
                .ForMember(dst => dst.NestedField, opt => opt.MapFrom(src => (int?)src.ToBeNestedField));

            CreateMap<EntityWithIntMembers, ModelWithEnumMembers>()
                .ForMember(e => e.EnumValue, o => o.MapFrom(s => (TestEnum)s.IntValue))
                .ForMember(e => e.NullableEnumValue, o => o.MapFrom(s => ((TestEnum)s.OtherIntValue) as TestEnum?));
        }
    }

    internal static class Helpers
    {
        //[DbFunction("Edm", "TruncateTime")]
        public static DateTime? TruncateTime(DateTime? date)
        {
            return date.HasValue ? date.Value.Date : (DateTime?)null;
        }
    }
}
