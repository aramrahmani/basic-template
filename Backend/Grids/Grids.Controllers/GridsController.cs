﻿using Common;
using Common.Security;
using Forms.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forms.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EnableCors("react")]
    public class GridsController : ControllerBase
    {
        private readonly IGridService service;
        private readonly IAzCheckService azCheck;


        public GridsController(IGridService formService, IAzCheckService azCheck)
        {
            this.service = formService;
            this.azCheck = azCheck;
        }


        [HttpPost("get-groups")]
        public Response<GroupInfoDTO> GetGroups(string projectId)
        {
            var userName = User.Identity.Name;
            var applicationId = "";
            var values = new Dictionary<string, string>();
            var groups = service.GetAllGroups(projectId);

            var az = groups
                .Select(x => x.AzView??"")
                .Distinct()
                .ToDictionary(x => x, x => azCheck.Validate(x, userName, applicationId, values).Length == 0);

            var grids = new List<Grid>();
            foreach (var g in groups)
            {
                foreach (var grid in service.GetGrids(projectId, g.Id))
                {
                    var az_view = grid.AzView ?? "";
                    if (!az.ContainsKey(az_view))
                    {
                        az.Add(az_view, azCheck.Validate(az_view, userName, applicationId, values).Length == 0);
                    }
                    if (az[az_view])
                        grids.Add(grid);
                }
            }

            var result = new GroupInfoDTO();
            result.Groups = groups.MapTo<GroupMenuDTO>();
            result.Grids = grids.MapTo<GridMenuDTO>();
            return new Response<GroupInfoDTO>(result);
        }



        [HttpPost("get-grid")]
        public Response<GridViewDTO> GetGrid(string projectId, string id)
        {
            var grid = service.GetGrid(projectId, id);

            if (!service.HasPermission(grid.AzView, User.Identity.Name, null))
                return new Response<GridViewDTO>(Messages.Forbidden, "403");

            var columns = service.GetGridColumns(grid.ProjectId, grid.Id);
            var variants = service.GetGridVariants(grid.ProjectId, grid.Id);

            var result = grid.MapTo<GridViewDTO>();
            result.CanInsert = string.IsNullOrEmpty(grid.AzInsert);
            result.CanUpdate = string.IsNullOrEmpty(grid.AzUpdate);
            result.CanDelete = string.IsNullOrEmpty(grid.AzDelete);
            result.DataColumns = columns.MapTo<GridColumnDTO>();
            result.Variants = variants.MapTo<GridVariantDTO>();
            return new Response<GridViewDTO>(result);
        }

        [HttpPost("get-grid-data")]
        public Response<IList<Dictionary<string, object>>> GetGridData(string projectId, string id, [FromBody] GridFilterDTO request)
        {
            var grid = service.GetGrid(projectId, id);
            var columns = service.GetGridColumns(grid.ProjectId, grid.Id);

            if (!service.HasPermission(grid.AzSelect, User.Identity.Name, null))
                return new Response<IList<Dictionary<string, object>>>(Messages.Forbidden);

            var data = service.ExecuteSelect(grid, columns, request.Filters, request.Parameters);
            return new Response<IList<Dictionary<string, object>>>(data.ToJSON());
        }


        [HttpPost("grid-insert")]
        public Response ExecGridInsert(string projectId, string id, [FromBody] Dictionary<string, object> values)
        {
            var grid = service.GetGrid(projectId, id);
            if (!service.HasPermission(grid.AzInsert, User.Identity.Name, values))
                return new Response(Messages.Forbidden);
            service.ExecuteInsert(grid, values);
            return new Response();
        }

        [HttpPost("grid-update")]
        public Response ExecGridUpdate(string projectId, string id, [FromBody] Dictionary<string, object> values)
        {
            var grid = service.GetGrid(projectId, id);
            if (!service.HasPermission(grid.AzUpdate, User.Identity.Name, values))
                return new Response(Messages.Forbidden);
            service.ExecuteUpdate(grid, values);
            return new Response();
        }

        [HttpPost("grid-delete")]
        public Response ExecGridDelete(string projectId, string id, [FromBody] Dictionary<string, object> values)
        {
            var grid = service.GetGrid(projectId, id);
            if (!service.HasPermission(grid.AzDelete, User.Identity.Name, values))
                return new Response(Messages.Forbidden);
            service.ExecuteDelete(grid, values);
            return new Response();
        }


        //[HttpPost("browse-grid")]
        //public Response<BrowseGridDTO> BrowseGrid(string projectId, string id)
        //{
        //    var filters = (Dictionary<string, object>)null;
        //    var grid = service.GetGrid(projectId, id);
        //    var columns = service.GetGridColumns(grid.ProjectId, grid.Id);
        //    var data = service.ExecuteSelect(grid, columns, filters, null);

        //    var result = new BrowseGridDTO();
        //    result.Schema = grid.MapTo<GridDTO>();
        //    result.Schema.DataColumns = columns.MapTo<GridColumnDTO>();
        //    result.Data = data.ToJSON();
        //    return new Response<BrowseGridDTO>(result);
        //}


        //[HttpPost("get-grid-schema")]
        //public Response<BrowseGridDTO> GetGridSchema(string projectId, string id)
        //{
        //    var grid = service.GetGrid(projectId, id);
        //    var columns = service.GetGridColumns(grid.ProjectId, grid.Id);

        //    var result = new BrowseGridDTO();
        //    result.Schema = grid.MapTo<GridDTO>();
        //    result.Schema.DataColumns = columns.MapTo<GridColumnDTO>();
        //    result.Schema.Variants = columns.MapTo<GridVariantDTO>();
        //    //result.Data = data.ToJSON();
        //    return new Response<BrowseGridDTO>(result);
        //}




        [HttpPost("grid-variant-save")]
        public Response<GridVariantDTO> SaveGridVariant(string projectId, string gridId, [FromBody] GridVariantDTO dto)
        {
            var variant = dto.MapTo<GridVariant>();
            variant.ProjectId = projectId;
            variant.GridId = gridId;
            var entity = service.SaveGridVariant(variant);
            var result = entity.MapTo<GridVariantDTO>();
            return new Response<GridVariantDTO>(result);
        }

        [HttpPost("grid-variants-update")]
        public Response UpdateVaraints(string projectId, string gridId, [FromBody] IList<UpdateVariantDTO> dto)
        {
            var variants = dto.MapTo<GridVariant>();
            foreach (var v in variants)
            {
                v.ProjectId = projectId;
                v.GridId = gridId;
            }
            service.UpdateGridVaraints(variants);
            return new Response();
        }

        [HttpPost("grid-variant-delete")]
        public Response DeleteGridVariant(int serial)
        {
            service.DeleteGridVariant(serial);
            return new Response();
        }


    }
}