﻿using System;
using System.Collections.Generic;
using System.Xml;

using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;

using Machine.Specifications.ReSharperRunner.Factories;

namespace Machine.Specifications.ReSharperRunner.Presentation
{
  public class BehaviorElement : FieldElement
  {
    readonly string _id;

    public BehaviorElement(MSpecUnitTestProvider provider,
                           PsiModuleManager psiModuleManager,
                           CacheManager cacheManager,
                           // ReSharper disable SuggestBaseTypeForParameter
                           ContextElement context,
                           // ReSharper restore SuggestBaseTypeForParameter
                           ProjectModelElementEnvoy projectEnvoy,
                           IClrTypeName declaringTypeName,
                           string fieldName,
                           bool isIgnored,
                           string fullyQualifiedTypeName)
      : base(
        provider,
        psiModuleManager,
        cacheManager,
        context,
        projectEnvoy,
        declaringTypeName,
        fieldName,
        isIgnored || context.Explicit)
    {
      FullyQualifiedTypeName = fullyQualifiedTypeName;
      _id = CreateId(context, fullyQualifiedTypeName, fieldName);
    }

    public ContextElement Context
    {
      get { return (ContextElement) Parent; }
    }

    public string FullyQualifiedTypeName { get; private set; }

    public override string Kind
    {
      get { return "Behavior"; }
    }

    public override IEnumerable<UnitTestElementCategory> Categories
    {
      get
      {
        var parent = Parent ?? Context;
        if (parent == null)
        {
          return UnitTestElementCategory.Uncategorized;
        }

        return parent.Categories;
      }
    }

    public override string Id
    {
      get { return _id; }
    }

    public override string GetTitlePrefix()
    {
      return "behaves like";
    }

    public override void WriteToXml(XmlElement parent)
    {
      base.WriteToXml(parent);
      parent.SetAttribute("typeFQN", FullyQualifiedTypeName);
    }

    public static IUnitTestElement ReadFromXml(XmlElement parent,
                                               IUnitTestElement parentElement,
                                               MSpecUnitTestProvider provider,
                                               ISolution solution,
                                               IUnitTestElementManager manager,
                                               PsiModuleManager psiModuleManager,
                                               CacheManager cacheManager
      )
    {
      var projectId = parent.GetAttribute("projectId");
      var project = ProjectUtil.FindProjectElementByPersistentID(solution, projectId) as IProject;
      if (project == null)
      {
        return null;
      }

      var context = parentElement as ContextElement;
      if (context == null)
      {
        return null;
      }

      var typeName = parent.GetAttribute("typeName");
      var methodName = parent.GetAttribute("methodName");
      var isIgnored = bool.Parse(parent.GetAttribute("isIgnored"));
      var fullyQualifiedTypeName = parent.GetAttribute("typeFQN");

      return BehaviorFactory.GetOrCreateBehavior(provider,
                                                 manager,
                                                 psiModuleManager,
                                                 cacheManager,
                                                 project,
                                                 ProjectModelElementEnvoy.Create(project),
                                                 context,
                                                 new ClrTypeName(typeName),
                                                 methodName,
                                                 isIgnored,
                                                 fullyQualifiedTypeName);
    }

    public static string CreateId(ContextElement contextElement, string fullyQualifiedTypeName, string fieldName)
    {
      return String.Format("{0}.{1}.{2}", contextElement.Id, fullyQualifiedTypeName, fieldName);
    }
  }
}